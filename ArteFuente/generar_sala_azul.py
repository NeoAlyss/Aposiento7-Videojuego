"""Genera el arte original del aposento azul y de los personajes (provisional, reemplazable).
Uso: python3 ArteFuente/generar_sala_azul.py   (requiere Pillow y numpy; usa generar_arte.py)"""
import math, random, sys, os
import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageChops
sys.path.insert(0, os.path.dirname(__file__))
from generar_arte import ventana_mask

A = 'Assets/Art/'
SS = 2                      # supermuestreo sobre el tamaño final
CONTORNO = (20, 14, 40, 255)

def lienzo(w, h): return Image.new('RGBA', (w * SS, h * SS), (0, 0, 0, 0))
PIXEL = 4                   # cada píxel del sprite ocupa 4x4 unidades de pantalla (se muestra con filtro Point)

def pixelar(im, ancho, alto, colores=14, contorno=True):
    """Convierte un dibujo grande en pixel art: reduce, endurece el borde, limita la paleta y repasa el contorno."""
    chico = im.resize((ancho, alto), Image.BOX)
    alfa = chico.split()[3].point(lambda v: 255 if v >= 110 else 0)
    rgb = chico.convert('RGB').quantize(colors=colores, method=Image.MEDIANCUT, dither=Image.NONE).convert('RGB')
    out = Image.new('RGBA', (ancho, alto), (0, 0, 0, 0)); out.paste(rgb, (0, 0), alfa)
    if contorno:
        borde = ImageChops.subtract(alfa.filter(ImageFilter.MaxFilter(3)), alfa)
        out.paste(Image.new('RGBA', (ancho, alto), CONTORNO), (0, 0), borde)
    return out

def fin(im, ruta, px, contorno=True, colores=14):
    """Guarda el dibujo como sprite de pixel art de px = (ancho, alto) píxeles."""
    pixelar(im, px[0], px[1], colores=colores, contorno=contorno).save(A + ruta, optimize=True)
def S(*v): return [x * SS for x in v]

class _Relleno:
    """ImageDraw que rellena de blanco por defecto (para dibujar máscaras sin repetir fill=255)."""
    def __init__(self, d): self._d = d
    def __getattr__(self, nombre):
        f = getattr(self._d, nombre)
        def llamar(*a, **k):
            if nombre != 'line': k.setdefault('fill', 255)
            return f(*a, **k)
        return llamar

def forma(im, dibujar, relleno, grosor=7):
    """Dibuja una forma con contorno grueso: primero la máscara, luego contorno dilatado + relleno."""
    m = Image.new('L', im.size, 0); dibujar(_Relleno(ImageDraw.Draw(m)))
    borde = m.filter(ImageFilter.MaxFilter(2 * grosor * SS // 2 * 2 + 1))
    im.paste(Image.new('RGBA', im.size, CONTORNO), (0, 0), borde)
    im.paste(Image.new('RGBA', im.size, relleno), (0, 0), m)
    return m

def sombrear(im, mascara, color, desplazar=(0.28, 0.0)):
    """Sombra plana en un costado de la forma (estilo recorte de papel)."""
    w, h = im.size
    lado = ImageChops.offset(mascara, int(-w * desplazar[0] * 0.2), int(h * desplazar[1] * 0.2))
    zona = ImageChops.subtract(mascara, lado)
    im.paste(Image.new('RGBA', im.size, color), (0, 0), zona)

# ------------------------------------------------------------------ fondo de la sala
def fondo():
    W, H = 1920, 1080
    pared = 430                                    # hasta aquí llega el muro del fondo
    yy, xx = np.mgrid[0:H, 0:W].astype(np.float32)
    img = np.zeros((H, W, 3), np.float32)
    # muro: azul profundo, más claro hacia el centro; suelo: azul noche
    cx = np.abs(xx - W / 2) / (W / 2)
    muro = (yy < pared)
    img[..., 0] = np.where(muro, 0.05 + 0.04 * (1 - cx), 0.030)
    img[..., 1] = np.where(muro, 0.10 + 0.10 * (1 - cx), 0.060)
    img[..., 2] = np.where(muro, 0.26 + 0.22 * (1 - cx), 0.160)
    # sillares del muro
    for fila in range(0, pared, 62):
        img[fila:fila + 3, :, :] *= 0.78
        corr = 0 if (fila // 62) % 2 == 0 else 80
        for x in range(corr, W, 160):
            img[fila:min(fila + 62, pared), x:x + 3, :] *= 0.80
    # zócalo
    img[pared - 14:pared + 6, :, :] = np.array([0.02, 0.04, 0.12])
    img[pared + 6:pared + 10, :, :] *= 1.6

    # vitrales azules
    ventanas = [(W * 0.26, pared - 40, 150, 330), (W * 0.5, pared - 40, 180, 370), (W * 0.74, pared - 40, 150, 330)]
    mask = Image.new('L', (W, H), 0)
    for v in ventanas: mask = ImageChops.lighter(mask, ventana_mask(W, H, *v))
    m = np.asarray(mask, np.float32) / 255
    halo = np.asarray(mask.filter(ImageFilter.GaussianBlur(70)), np.float32) / 255
    halo2 = np.asarray(mask.filter(ImageFilter.GaussianBlur(22)), np.float32) / 255
    img += np.stack([halo * 0.10 + halo2 * 0.10, halo * 0.34 + halo2 * 0.30, halo * 0.95 + halo2 * 0.55], -1)
    borde = np.asarray(mask.filter(ImageFilter.GaussianBlur(8)), np.float32) / 255
    vidrio = np.stack([0.30 + 0.55 * borde ** 3, 0.62 + 0.33 * borde ** 2, np.ones_like(m)], -1)
    img = img * (1 - m[..., None]) + vidrio * m[..., None]

    # la luz de cada vitral cae sobre el suelo
    luz = np.zeros((H, W), np.float32)
    for (vx, by, a, al), incl in zip(ventanas, (-0.35, 0.0, 0.35)):
        t = np.clip((yy - pared) / (H - pared), 0, 1)
        centro = vx + incl * (yy - pared)
        ancho = a * 0.7 + t * 260
        luz += np.exp(-((xx - centro) / ancho) ** 2) * (1 - t * 0.75) * (yy >= pared) * 0.20
    img += np.stack([luz * 0.25, luz * 0.60, luz * 1.0], -1)

    # cortinajes oscuros a los lados y viñeta
    for lado in (0, 1):
        x0 = xx if lado == 0 else (W - xx)
        cort = np.clip(1 - x0 / (150 + 40 * np.sin(yy / 90.0)), 0, 1) ** 0.8
        img *= (1 - 0.85 * cort)[..., None]
    v = np.clip(1.10 - np.hypot((xx - W / 2) / (W * 0.66), (yy - H * 0.55) / (H * 0.80)) ** 2.6 * 0.8, 0.15, 1)
    img *= v[..., None]
    grande = Image.fromarray((np.clip(img, 0, 1) * 255).astype(np.uint8), 'RGB')
    chico = grande.resize((W // PIXEL, H // PIXEL), Image.BOX).quantize(colors=40, method=Image.MEDIANCUT, dither=Image.NONE)
    chico.convert('RGB').save(A + 'Rooms/Blue/sala_azul_fondo.png', optimize=True)

# ------------------------------------------------------------------ baldosa (blanca, se tiñe en Unity)
def baldosa():
    w, h = 300, 208
    im = lienzo(w, h); d = ImageDraw.Draw(im)
    d.rounded_rectangle(S(6, 16, w - 6, h - 4), radius=20 * SS, fill=(120, 120, 130, 255))      # canto (grosor)
    d.rounded_rectangle(S(6, 4, w - 6, h - 22), radius=20 * SS, fill=(236, 236, 240, 255))      # cara superior
    d.rounded_rectangle(S(18, 14, w - 18, h - 32), radius=12 * SS, outline=(255, 255, 255, 255), width=3 * SS)
    d.line(S(30, h - 40, w - 30, h - 40), fill=(205, 205, 212, 255), width=3 * SS)
    fin(im, 'Rooms/Blue/baldosa.png', (38, 26), contorno=False)

# ------------------------------------------------------------------ personajes (lienzo 360 x 460, pies abajo)
# Paletas de la túnica: (base, sombra, luz)
ROJO = ((172, 30, 38, 255), (118, 16, 28, 255), (206, 58, 60, 255))
NEGRO = ((50, 48, 60, 255), (30, 28, 38, 255), (80, 78, 94, 255))

def figura(direccion, f, paleta, mascara=None):
    """Encapuchado en pixel art nativo (38 x 48). direccion: sur (de frente), norte (de espaldas) o este
    (de lado; el oeste es su espejo). f: cuadro de caminata 0..3. Sin máscara, donde iría la cara hay
    un hueco oscuro. Diseño propio."""
    W, H = 38, 48
    R1, R2, R3 = paleta
    V, V2, G = (16, 10, 14, 255), (40, 34, 42, 255), (58, 54, 62, 255)
    im = Image.new('RGBA', (W, H), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    bob = 1 if f in (1, 3) else 0                 # sube y baja al caminar
    vaiven = (0, 1, 0, -1)[f]                      # el ruedo del manto se mece
    brazo = (0, 1, 0, -1)[f]
    y0 = bob
    lado = direccion == 'este'
    # manto
    if lado:
        d.polygon([(12, 19 + y0), (25, 19 + y0), (28 + vaiven, 45), (9 + vaiven, 45)], fill=R1)
        d.polygon([(21, 20 + y0), (25, 19 + y0), (28 + vaiven, 45), (23 + vaiven, 45)], fill=R2)
    else:
        d.polygon([(10, 19 + y0), (27, 19 + y0), (31 + vaiven, 45), (6 + vaiven, 45)], fill=R1)
        d.polygon([(23, 20 + y0), (27, 19 + y0), (31 + vaiven, 45), (25 + vaiven, 45)], fill=R2)
    for x in range(7, 31, 4): d.point((x + vaiven, 45), fill=(0, 0, 0, 0))      # ruedo deshilachado
    # capucha
    cx = 20 if lado else 18
    d.ellipse([cx - 9, 3 + y0, cx + 9, 22 + y0], fill=R1)
    d.polygon([(cx - 3, 5 + y0), (cx + 1, 1 + y0), (cx + 4, 6 + y0)], fill=R1)
    d.arc([cx - 9, 3 + y0, cx + 9, 22 + y0], 300, 70, fill=R2, width=2)
    d.arc([cx - 8, 4 + y0, cx + 8, 21 + y0], 190, 260, fill=R3, width=1)
    if direccion == 'sur':
        d.ellipse([cx - 5, 8 + y0, cx + 5, 20 + y0], fill=V)                    # el hueco: no hay cara
        d.ellipse([cx - 3, 13 + y0, cx + 3, 19 + y0], fill=V2)
        if mascara: mascara(d, cx, y0)
        d.rectangle([17, 23 + y0, 19, 44], fill=V)                            # abertura del manto
        for y in range(25 + y0, 44, 3): d.point((18, y), fill=V2)
        # mangas anchas y guantes
        d.polygon([(8, 21 + y0), (12, 21 + y0), (11, 34 + y0 + brazo), (5, 34 + y0 + brazo)], fill=R2)
        d.polygon([(25, 21 + y0), (29, 21 + y0), (32, 34 + y0 - brazo), (26, 34 + y0 - brazo)], fill=R2)
        d.rectangle([6, 34 + y0 + brazo, 10, 36 + y0 + brazo], fill=V); d.point((7, 34 + y0 + brazo), fill=G)
        d.rectangle([27, 34 + y0 - brazo, 31, 36 + y0 - brazo], fill=V); d.point((30, 34 + y0 - brazo), fill=G)
    elif direccion == 'norte':
        d.line([(cx, 6 + y0), (cx, 20 + y0)], fill=R2)                          # costura de la capucha
        d.line([(10, 24 + y0), (27, 24 + y0)], fill=R2)                          # esclavina
        d.polygon([(8, 21 + y0), (11, 21 + y0), (10, 33 + y0 - brazo), (5, 33 + y0 - brazo)], fill=R2)
        d.polygon([(26, 21 + y0), (29, 21 + y0), (32, 33 + y0 + brazo), (27, 33 + y0 + brazo)], fill=R2)
    else:
        d.ellipse([cx + 2, 8 + y0, cx + 9, 20 + y0], fill=V)
        d.ellipse([cx + 4, 13 + y0, cx + 8, 19 + y0], fill=V2)
        d.rectangle([cx + 8, 4 + y0, cx + 10, 22 + y0], fill=(0, 0, 0, 0))     # recorta el frente de la capucha
        d.line([(cx + 7, 6 + y0), (cx + 7, 21 + y0)], fill=R1)
        ax = 19 + brazo * 2
        d.polygon([(15, 22 + y0), (20, 22 + y0), (ax + 5, 34 + y0), (ax - 1, 34 + y0)], fill=R2)
        d.rectangle([ax + 1, 34 + y0, ax + 5, 36 + y0], fill=V); d.point((ax + 4, 34 + y0), fill=G)
    # contorno de 1 px alrededor de la silueta
    alfa = im.split()[3].point(lambda v: 255 if v > 0 else 0)
    borde = ImageChops.subtract(alfa.filter(ImageFilter.MaxFilter(3)), alfa)
    im.paste(Image.new('RGBA', (W, H), CONTORNO), (0, 0), borde)
    return im

def encapuchado():
    """Protagonista: capucha roja, sin cara. 4 direcciones x 4 cuadros de caminata."""
    os.makedirs(A + 'Characters/Encapuchado', exist_ok=True)
    for direccion in ('sur', 'norte', 'este', 'oeste'):
        for f in range(4):
            im = figura('este' if direccion == 'oeste' else direccion, f, ROJO)
            if direccion == 'oeste': im = im.transpose(Image.FLIP_LEFT_RIGHT)
            im.save(A + f'Characters/Encapuchado/encapuchado_{direccion}_{f}.png', optimize=True)

# ---- máscaras de los invitados (diseños propios, genéricos): se pintan sobre el hueco de la capucha
OSC = (20, 14, 26, 255)

def mascara_gato(d, cx, y0):
    hueso = (240, 236, 226, 255)
    d.rectangle([cx - 4, 10 + y0, cx + 4, 16 + y0], fill=hueso)
    d.polygon([(cx - 4, 10 + y0), (cx - 4, 7 + y0), (cx - 2, 10 + y0)], fill=hueso)      # orejas
    d.polygon([(cx + 4, 10 + y0), (cx + 4, 7 + y0), (cx + 2, 10 + y0)], fill=hueso)
    d.line([(cx - 3, 17 + y0), (cx + 3, 17 + y0)], fill=hueso)
    d.line([(cx - 3, 13 + y0), (cx - 2, 12 + y0)], fill=OSC); d.line([(cx + 2, 12 + y0), (cx + 3, 13 + y0)], fill=OSC)   # ojos rasgados
    d.point((cx, 15 + y0), fill=(226, 120, 140, 255))                                    # nariz
    d.point((cx - 5, 15 + y0), fill=hueso); d.point((cx + 5, 15 + y0), fill=hueso)       # bigotes

def mascara_zorro(d, cx, y0):
    naranja, blanco = (232, 118, 44, 255), (248, 240, 228, 255)
    d.polygon([(cx - 5, 10 + y0), (cx + 5, 10 + y0), (cx + 3, 15 + y0), (cx, 19 + y0), (cx - 3, 15 + y0)], fill=naranja)
    d.polygon([(cx - 5, 10 + y0), (cx - 5, 6 + y0), (cx - 2, 10 + y0)], fill=naranja)    # orejas altas
    d.polygon([(cx + 5, 10 + y0), (cx + 5, 6 + y0), (cx + 2, 10 + y0)], fill=naranja)
    d.polygon([(cx - 2, 15 + y0), (cx + 2, 15 + y0), (cx, 18 + y0)], fill=blanco)        # hocico
    d.point((cx, 19 + y0), fill=OSC)
    d.point((cx - 3, 12 + y0), fill=OSC); d.point((cx + 3, 12 + y0), fill=OSC)

def mascara_halcon(d, cx, y0):
    oro, oro_osc, ojo = (238, 192, 76, 255), (176, 124, 36, 255), (120, 220, 240, 255)
    d.rectangle([cx - 5, 11 + y0, cx + 5, 14 + y0], fill=oro)                            # antifaz
    d.line([(cx - 6, 10 + y0), (cx - 4, 10 + y0)], fill=oro); d.line([(cx + 4, 10 + y0), (cx + 6, 10 + y0)], fill=oro)   # cejas en ala
    d.point((cx - 7, 9 + y0), fill=oro); d.point((cx + 7, 9 + y0), fill=oro)
    d.line([(cx - 4, 12 + y0), (cx - 2, 13 + y0)], fill=OSC); d.line([(cx + 2, 13 + y0), (cx + 4, 12 + y0)], fill=OSC)   # mirada fija
    d.point((cx - 3, 13 + y0), fill=ojo); d.point((cx + 3, 13 + y0), fill=ojo)
    d.polygon([(cx - 1, 14 + y0), (cx + 1, 14 + y0), (cx, 17 + y0)], fill=oro_osc)        # pico

def invitados():
    """Los invitados: la misma figura con capucha negra, cada uno con su máscara."""
    for i, mascara in enumerate((mascara_gato, mascara_zorro, mascara_halcon), start=1):
        figura('sur', 0, NEGRO, mascara).save(A + f'Characters/invitado_{i}.png', optimize=True)

def sombra():
    im = Image.new('RGBA', (32, 12), (0, 0, 0, 0))
    ImageDraw.Draw(im).ellipse([1, 1, 30, 10], fill=(0, 0, 0, 150))
    im.save(A + 'Props/sombra.png', optimize=True)

# ------------------------------------------------------------------ objetos
def llave():
    w, h = 200, 200
    im = lienzo(w, h); oro, oro_osc = (246, 200, 76, 255), (196, 140, 40, 255)
    def k(d):
        d.ellipse(S(22, 62, 98, 138)); d.rectangle(S(88, 88, 180, 112)); d.rectangle(S(138, 108, 154, 142)); d.rectangle(S(164, 108, 180, 134))
    m = forma(im, k, oro, grosor=6); sombrear(im, m, oro_osc, (0.0, 0.5))
    forma(im, lambda d: d.ellipse(S(44, 84, 76, 116)), (0, 0, 0, 0), grosor=6)
    d = ImageDraw.Draw(im); d.ellipse(S(44, 84, 76, 116), fill=(0, 0, 0, 0))
    im = im.rotate(-28, resample=Image.BICUBIC)
    fin(im, 'Props/llave.png', (21, 21))

def puerta(abierta):
    w, h = 340, 470
    im = lienzo(w, h)
    piedra, piedra_osc = (92, 110, 168, 255), (60, 74, 124, 255)
    def marco(d): d.rounded_rectangle(S(20, 20, w - 20, h + 60), radius=150 * SS)
    m = forma(im, marco, piedra); sombrear(im, m, piedra_osc)
    def hueco(d): d.rounded_rectangle(S(60, 62, w - 60, h + 60), radius=110 * SS)
    if abierta:
        mh = forma(im, hueco, (40, 10, 60, 255), grosor=5)
        # del otro lado asoma el aposento siguiente (púrpura)
        yy, xx = np.mgrid[0:im.height, 0:im.width].astype(np.float32)
        g = np.clip(1 - np.hypot((xx - im.width / 2) / (im.width * 0.30), (yy - im.height * 0.62) / (im.height * 0.45)), 0, 1)
        luz = np.stack([120 + 135 * g, 40 + 150 * g, 170 + 85 * g, 255 * np.ones_like(g)], -1).astype(np.uint8)
        im.paste(Image.fromarray(luz, 'RGBA'), (0, 0), mh)
    else:
        madera, madera_osc = (70, 52, 84, 255), (46, 32, 60, 255)
        mh = forma(im, hueco, madera, grosor=5); sombrear(im, mh, madera_osc)
        d = ImageDraw.Draw(im)
        d.line(S(w / 2, 70, w / 2, h), fill=madera_osc, width=6 * SS)
        for y in (190, 330): d.rectangle(S(64, y, w - 64, y + 22), fill=(36, 40, 70, 255))
        # tres cerraduras: una por llave
        for i, x in enumerate((w / 2 - 62, w / 2, w / 2 + 62)):
            forma(im, lambda dd, x=x: dd.ellipse(S(x - 20, 244, x + 20, 284)), (236, 190, 80, 255), grosor=4)
            d = ImageDraw.Draw(im); d.ellipse(S(x - 7, 254, x + 7, 268), fill=CONTORNO); d.rectangle(S(x - 3, 262, x + 3, 278), fill=CONTORNO)
    im = im.crop((0, 0, im.width, h * SS))
    fin(im, 'Props/puerta_abierta.png' if abierta else 'Props/puerta_cerrada.png', (49, 68), colores=20)

def globo():
    w, h = 760, 300
    im = lienzo(w, h)
    def g(d):
        d.rounded_rectangle(S(12, 12, w - 12, h - 70), radius=60 * SS)
        d.polygon(S(w / 2 - 34, h - 76, w / 2 + 34, h - 76, w / 2, h - 14))
    forma(im, g, (252, 250, 244, 255), grosor=6)
    fin(im, 'Props/globo.png', (85, 34), colores=4)

if __name__ == '__main__':
    fondo(); baldosa(); encapuchado(); sombra(); llave(); puerta(False); puerta(True); globo()
    invitados()
