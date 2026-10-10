"""Genera el arte del aposento negro (capítulo 1): fondo panorámico, reloj de ébano, puerta,
muebles e invitados con máscaras de colores. Mismo estilo pixel art que el aposento azul.
Uso: python3 ArteFuente/generar_sala_negra.py   (requiere Pillow y numpy; usa generar_sala_azul.py)"""
import os, sys, math
import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageChops
sys.path.insert(0, os.path.dirname(__file__))
from generar_arte import ventana_mask
from generar_sala_azul import (A, SS, CONTORNO, PIXEL, NEGRO, figura, forma, sombrear, lienzo, S, fin,
                               mascara_gato, mascara_zorro, mascara_halcon)

CARPETA = 'Rooms/Black/'

def contorno(im):
    """Contorno de 1 px alrededor de la silueta (como los personajes)."""
    alfa = im.split()[3].point(lambda v: 255 if v > 0 else 0)
    borde = ImageChops.subtract(alfa.filter(ImageFilter.MaxFilter(3)), alfa)
    im.paste(Image.new('RGBA', im.size, CONTORNO), (0, 0), borde)
    return im

# ------------------------------------------------------------------ fondo panorámico
def fondo():
    """3840 x 1080 unidades (4 pantallas de ancho... de alto 1): muro de terciopelo negro, siete
    vitrales escarlata encendidos desde atrás por braseros, y su luz roja sobre el suelo."""
    W, H = 3840, 1080
    pared = 430
    yy, xx = np.mgrid[0:H, 0:W].astype(np.float32)
    img = np.zeros((H, W, 3), np.float32)
    muro = yy < pared
    # terciopelo: pliegues verticales suaves
    pliegue = 0.5 + 0.5 * np.sin(xx / 23.0 + np.sin(xx / 97.0) * 2.0)
    img[..., 0] = np.where(muro, 0.030 + 0.020 * pliegue, 0.026)
    img[..., 1] = np.where(muro, 0.018 + 0.010 * pliegue, 0.014)
    img[..., 2] = np.where(muro, 0.026 + 0.016 * pliegue, 0.020)
    # cenefa dorada apagada arriba y zócalo
    img[18:26, :, :] = np.array([0.20, 0.13, 0.05])
    img[pared - 16:pared + 6, :, :] = np.array([0.05, 0.02, 0.03])
    img[pared + 6:pared + 10, :, :] = np.array([0.16, 0.05, 0.06])

    # vitrales escarlata
    xs = np.linspace(330, W - 330, 7)
    ventanas = [(x, pared - 40, 150 if i % 2 else 170, 330 if i % 2 else 360) for i, x in enumerate(xs)]
    mask = Image.new('L', (W, H), 0)
    for v in ventanas: mask = ImageChops.lighter(mask, ventana_mask(W, H, *v))
    m = np.asarray(mask, np.float32) / 255
    halo = np.asarray(mask.filter(ImageFilter.GaussianBlur(80)), np.float32) / 255
    halo2 = np.asarray(mask.filter(ImageFilter.GaussianBlur(24)), np.float32) / 255
    img += np.stack([halo * 0.70 + halo2 * 0.45, halo * 0.06 + halo2 * 0.05, halo * 0.08 + halo2 * 0.06], -1)
    borde = np.asarray(mask.filter(ImageFilter.GaussianBlur(9)), np.float32) / 255
    # vidrio: rojo sangre, más encendido en el centro (el brasero detrás)
    centro = np.clip(borde, 0, 1) ** 2
    vidrio = np.stack([0.55 + 0.45 * centro, 0.03 + 0.20 * centro ** 3, 0.05 + 0.10 * centro ** 3], -1)
    img = img * (1 - m[..., None]) + vidrio * m[..., None]

    # haces de luz roja sobre el suelo
    luz = np.zeros((H, W), np.float32)
    t = np.clip((yy - pared) / (H - pared), 0, 1)
    for i, (vx, by, a, al) in enumerate(ventanas):
        incl = (vx - W / 2) / (W / 2) * 0.30
        c = vx + incl * (yy - pared)
        ancho = a * 0.7 + t * 240
        luz += np.exp(-((xx - c) / ancho) ** 2) * (1 - t * 0.7) * (yy >= pared) * 0.26
    img += np.stack([luz * 1.0, luz * 0.10, luz * 0.12], -1)

    # cortinajes a los extremos y viñeta vertical (sin oscurecer el centro de cada pantalla)
    for lado in (0, 1):
        x0 = xx if lado == 0 else (W - xx)
        cort = np.clip(1 - x0 / (120 + 30 * np.sin(yy / 80.0)), 0, 1) ** 0.8
        img *= (1 - 0.9 * cort)[..., None]
    v = np.clip(1.08 - (np.abs(yy - H * 0.55) / (H * 0.62)) ** 2.4 * 0.9, 0.2, 1)
    img *= v[..., None]

    grande = Image.fromarray((np.clip(img, 0, 1) * 255).astype(np.uint8), 'RGB')
    chico = grande.resize((W // PIXEL, H // PIXEL), Image.BOX).quantize(colors=40, method=Image.MEDIANCUT, dither=Image.NONE)
    os.makedirs(A + CARPETA, exist_ok=True)
    chico.convert('RGB').save(A + CARPETA + 'sala_negra_fondo.png', optimize=True)

# ------------------------------------------------------------------ reloj de pie de ébano (40 x 92 px)
EB1, EB2, EB3 = (40, 30, 36, 255), (24, 18, 22, 255), (70, 54, 62, 255)     # ébano: base, sombra, brillo
BRONCE, BRONCE2 = (212, 164, 72, 255), (150, 104, 40, 255)
MARFIL = (228, 216, 188, 255)
HUECO = (12, 6, 10, 255)

def reloj(cuadro=0, abierto=False):
    """Reloj de pie de ébano, con la esfera marcando la medianoche. cuadro 0..3: posición del
    péndulo (oscila detrás del vidrio). abierto: la portezuela del tronco abierta, por donde sale
    el encapuchado."""
    W, H = 40, 92
    im = Image.new('RGBA', (W, H), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    # pedestal
    d.rectangle([5, 82, 34, 90], fill=EB1); d.rectangle([5, 82, 34, 83], fill=EB3)
    d.rectangle([7, 90, 32, 91], fill=EB2)
    for x in (6, 33): d.rectangle([x - 1, 89, x, 91], fill=BRONCE2)               # patas
    # tronco
    d.rectangle([9, 36, 30, 82], fill=EB1)
    d.rectangle([27, 36, 30, 82], fill=EB2)                                         # costado en sombra
    d.line([(10, 36), (10, 82)], fill=EB3)
    # cabeza (capitel) con frontón curvo y remate
    d.rectangle([6, 8, 33, 36], fill=EB1)
    d.rectangle([30, 8, 33, 36], fill=EB2)
    d.pieslice([6, 0, 33, 18], 180, 360, fill=EB1)
    d.arc([6, 0, 33, 18], 180, 360, fill=EB3)
    d.rectangle([18, 0, 21, 3], fill=BRONCE); d.point((19, 0), fill=MARFIL)         # remate
    d.rectangle([5, 34, 34, 37], fill=EB2); d.line([(5, 34), (34, 34)], fill=EB3)   # moldura
    # esfera: medianoche
    cx, cy, r = 19.5, 21, 9
    d.ellipse([cx - r - 1, cy - r - 1, cx + r + 1, cy + r + 1], fill=BRONCE2)
    d.ellipse([cx - r, cy - r, cx + r, cy + r], fill=MARFIL)
    for h in range(12):
        a = h / 12 * 2 * math.pi
        x, y = cx + math.sin(a) * (r - 1.5), cy - math.cos(a) * (r - 1.5)
        d.point((round(x), round(y)), fill=HUECO)
    d.line([(19, cy), (19, cy - 7)], fill=HUECO); d.line([(20, cy), (20, cy - 5)], fill=HUECO)   # manillas en las XII
    d.point((19, cy), fill=BRONCE2); d.point((20, cy), fill=BRONCE2)
    # portezuela del tronco con ventana
    if not abierto:
        d.rectangle([12, 40, 27, 78], fill=EB2)
        d.rectangle([13, 41, 26, 77], fill=EB1)
        d.rectangle([15, 44, 24, 73], fill=HUECO)                                   # vidrio oscuro
        d.line([(15, 44), (15, 50)], fill=(80, 40, 50, 255))                         # reflejo
        # péndulo
        dx = (-3, 0, 3, 0)[cuadro]
        d.line([(19 + dx // 2, 44), (19 + dx, 66)], fill=BRONCE2)
        d.line([(20 + dx // 2, 44), (20 + dx, 66)], fill=BRONCE2)
        d.ellipse([16 + dx, 64, 23 + dx, 71], fill=BRONCE); d.point((18 + dx, 66), fill=MARFIL)
        d.point((26, 60), fill=BRONCE)                                              # cerradura
    else:
        # adentro: oscuridad con un resplandor rojo desde el fondo
        d.rectangle([12, 40, 27, 78], fill=HUECO)
        for i, c in enumerate([(60, 8, 14, 255), (90, 12, 20, 255), (120, 18, 26, 255)]):
            d.ellipse([15 + i, 56 + i * 3, 24 - i, 76 - i], fill=c)
        # la portezuela, abierta hacia afuera (se ve de canto a la derecha)
        d.polygon([(28, 40), (33, 43), (33, 81), (28, 78)], fill=EB2)
        d.line([(33, 43), (33, 81)], fill=EB3)
    return contorno(im)

def relojes():
    for c in range(4): reloj(c).save(A + f'Props/reloj_ebano_{c}.png', optimize=True)
    reloj(abierto=True).save(A + 'Props/reloj_ebano_abierto.png', optimize=True)

# ------------------------------------------------------------------ puerta de salida (ébano y hierro)
def puerta(abierta):
    w, h = 340, 470
    im = lienzo(w, h)
    piedra, piedra_osc = (64, 40, 50, 255), (40, 24, 32, 255)
    def marco(d): d.rounded_rectangle(S(20, 20, w - 20, h + 60), radius=150 * SS)
    m = forma(im, marco, piedra); sombrear(im, m, piedra_osc)
    def hueco(d): d.rounded_rectangle(S(60, 62, w - 60, h + 60), radius=110 * SS)
    if abierta:
        mh = forma(im, hueco, (20, 6, 16, 255), grosor=5)
        # del otro lado: el aposento violeta, en penumbra
        yy, xx = np.mgrid[0:im.height, 0:im.width].astype(np.float32)
        g = np.clip(1 - np.hypot((xx - im.width / 2) / (im.width * 0.30), (yy - im.height * 0.62) / (im.height * 0.45)), 0, 1)
        luz = np.stack([60 + 110 * g, 20 + 50 * g, 90 + 120 * g, 255 * np.ones_like(g)], -1).astype(np.uint8)
        im.paste(Image.fromarray(luz, 'RGBA'), (0, 0), mh)
    else:
        madera, madera_osc = (36, 26, 32, 255), (22, 14, 20, 255)
        mh = forma(im, hueco, madera, grosor=5); sombrear(im, mh, madera_osc)
        d = ImageDraw.Draw(im)
        d.line(S(w / 2, 70, w / 2, h), fill=madera_osc, width=6 * SS)
        for y in (190, 330): d.rectangle(S(64, y, w - 64, y + 22), fill=(70, 20, 28, 255))
        for i, x in enumerate((w / 2 - 62, w / 2, w / 2 + 62)):               # tres cerraduras: una por llave
            forma(im, lambda dd, x=x: dd.ellipse(S(x - 20, 244, x + 20, 284)), (236, 190, 80, 255), grosor=4)
            d = ImageDraw.Draw(im); d.ellipse(S(x - 7, 254, x + 7, 268), fill=CONTORNO); d.rectangle(S(x - 3, 262, x + 3, 278), fill=CONTORNO)
    im = im.crop((0, 0, im.width, h * SS))
    fin(im, CARPETA + ('puerta_abierta.png' if abierta else 'puerta_cerrada.png'), (49, 68), colores=20)


# ------------------------------------------------------------------ muebles (pixel art nativo, se muestran a 3x)
MAD1, MAD2, MAD3 = (66, 36, 30, 255), (42, 22, 20, 255), (98, 58, 44, 255)       # madera oscura
TERC1, TERC2, TERC3 = (150, 24, 36, 255), (98, 14, 26, 255), (190, 50, 58, 255)   # terciopelo carmesí
HOJA1, HOJA2, HOJA3 = (42, 92, 56, 255), (26, 60, 38, 255), (74, 130, 80, 255)
LLAMA, LLAMA2 = (255, 214, 120, 255), (240, 140, 50, 255)
CERA = (232, 220, 196, 255)

def libreria():
    """Librería de 2 baldosas de ancho (96 x 70)."""
    W, H = 96, 70
    im = Image.new('RGBA', (W, H), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    d.rectangle([2, 4, 93, 69], fill=MAD1)
    d.rectangle([0, 0, 95, 5], fill=MAD3); d.rectangle([0, 4, 95, 6], fill=MAD2)    # cornisa
    d.rectangle([88, 6, 93, 69], fill=MAD2)
    lomos = [(120, 30, 36), (60, 80, 110), (150, 112, 50), (52, 90, 60), (96, 40, 90), (170, 150, 120), (80, 30, 30)]
    estantes = [8, 28, 48]
    k = 0
    for y0 in estantes:
        d.rectangle([5, y0, 86, y0 + 17], fill=(20, 10, 12, 255))                  # fondo del estante
        x = 6
        while x < 84:
            ancho = 2 + (k * 7) % 3
            alto = 12 + (k * 5) % 5
            if (k * 11) % 13 == 0:                                                   # un libro caído
                d.rectangle([x, y0 + 17 - 3, x + 8, y0 + 16], fill=lomos[k % len(lomos)] + (255,))
                x += 10
            else:
                c = lomos[k % len(lomos)]
                d.rectangle([x, y0 + 17 - alto, x + ancho, y0 + 16], fill=c + (255,))
                d.point((x, y0 + 17 - alto + 2), fill=(230, 196, 100, 255))         # letras doradas
                x += ancho + 1
            k += 1
        d.rectangle([3, y0 + 17, 88, y0 + 19], fill=MAD3)                            # tabla
    d.rectangle([2, 66, 93, 69], fill=MAD2)
    return contorno(im)

def sillon():
    """Sillón de terciopelo carmesí, 2 baldosas (96 x 46)."""
    W, H = 96, 46
    im = Image.new('RGBA', (W, H), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    d.rounded_rectangle([6, 2, 89, 26], radius=8, fill=TERC1)                       # respaldo
    d.rounded_rectangle([6, 2, 89, 8], radius=6, fill=TERC3)
    for x in range(16, 86, 12):                                                      # capitoné
        for y in (10, 18): d.point((x, y), fill=TERC2)
    d.rectangle([10, 24, 85, 36], fill=TERC1); d.line([(10, 24), (85, 24)], fill=TERC3)   # asiento
    d.line([(48, 25), (48, 36)], fill=TERC2)
    for x0 in (0, 84):                                                               # brazos
        d.rounded_rectangle([x0, 14, x0 + 11, 38], radius=5, fill=TERC1)
        d.rounded_rectangle([x0, 14, x0 + 11, 19], radius=4, fill=TERC3)
    d.rectangle([4, 36, 91, 40], fill=TERC2)
    d.rectangle([6, 40, 9, 45], fill=MAD2); d.rectangle([86, 40, 89, 45], fill=MAD2)    # patas
    d.point((7, 44), fill=(212, 164, 72, 255)); d.point((87, 44), fill=(212, 164, 72, 255))
    return contorno(im)

def planta():
    """Palmera en maceta de bronce (32 x 54)."""
    W, H = 32, 54
    im = Image.new('RGBA', (W, H), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    for ang, largo, c in [(-150, 17, HOJA2), (-30, 17, HOJA2), (-120, 19, HOJA1), (-60, 19, HOJA1), (-95, 20, HOJA3), (-80, 18, HOJA1)]:
        a = math.radians(ang)
        x1, y1 = 16 + math.cos(a) * largo, 30 + math.sin(a) * largo
        d.line([(16, 30), (x1, y1)], fill=c, width=3)
        d.line([(x1, y1), (x1 + math.cos(a) * 3, y1 + 4)], fill=c, width=2)          # punta caída
    d.line([(16, 30), (16, 37)], fill=HOJA2, width=2)
    d.polygon([(7, 36), (25, 36), (22, 52), (10, 52)], fill=BRONCE2)                  # maceta
    d.rectangle([6, 35, 26, 38], fill=BRONCE); d.line([(9, 44), (23, 44)], fill=BRONCE)
    return contorno(im)

def mesita():
    """Mesita redonda con una vela y una copa (34 x 38)."""
    W, H = 34, 38
    im = Image.new('RGBA', (W, H), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    d.ellipse([2, 14, 31, 22], fill=MAD3); d.ellipse([2, 16, 31, 24], fill=MAD1)        # tablero
    d.rectangle([15, 23, 18, 34], fill=MAD2)                                         # pie
    d.polygon([(8, 37), (25, 37), (18, 32), (15, 32)], fill=MAD2)
    d.rectangle([9, 6, 11, 16], fill=CERA); d.point((10, 4), fill=LLAMA); d.point((10, 5), fill=LLAMA2)   # vela
    d.point((10, 3), fill=LLAMA)
    d.polygon([(20, 8), (26, 8), (24, 12), (22, 12)], fill=(200, 200, 210, 255))     # copa
    d.line([(23, 12), (23, 16)], fill=(200, 200, 210, 255)); d.point((22, 9), fill=(150, 20, 30, 255)); d.point((23, 9), fill=(150, 20, 30, 255))
    return contorno(im)

def candelabro():
    """Candelabro de hierro de pie con tres velas encendidas (28 x 68)."""
    W, H = 28, 68
    im = Image.new('RGBA', (W, H), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    HIE, HIE2 = (70, 64, 72, 255), (40, 36, 44, 255)
    d.rectangle([13, 18, 14, 62], fill=HIE)
    d.polygon([(6, 67), (21, 67), (15, 60), (12, 60)], fill=HIE2)
    d.arc([3, 8, 24, 26], 0, 180, fill=HIE, width=2)                                  # brazos
    for x in (3, 13, 23):
        d.rectangle([x, 8, x + 1, 17], fill=CERA)
        d.point((x, 6), fill=LLAMA); d.point((x + 1, 6), fill=LLAMA); d.point((x, 7), fill=LLAMA2); d.point((x + 1, 7), fill=LLAMA2)
        d.point((x, 5), fill=LLAMA)
    return contorno(im)

def muebles():
    for nombre, f in (('libreria', libreria), ('sillon', sillon), ('planta', planta), ('mesita', mesita), ('candelabro', candelabro)):
        f().save(A + f'Props/{nombre}.png', optimize=True)

# ------------------------------------------------------------------ invitados con máscaras de colores
HUESO, NARANJA, ORO, ORO_OSC = (240, 236, 226), (232, 118, 44), (238, 192, 76), (176, 124, 36)
COLORES = [  # (base, sombra) de cada color de máscara; el 0 es el original
    None,
    ((200, 208, 220), (124, 134, 150)),   # plata
    ((206, 44, 56), (132, 22, 34)),       # carmesí
    ((90, 188, 142), (42, 112, 86)),      # jade
    ((156, 100, 212), (92, 54, 144)),     # violeta
]

def invitados():
    """invitado_NN.png con NN = máscara + 3 * color (gato, zorro, halcón × 5 colores)."""
    os.makedirs(A + 'Characters/Invitados', exist_ok=True)
    for color, par in enumerate(COLORES):
        for mascara, dibujar in enumerate((mascara_gato, mascara_zorro, mascara_halcon)):
            im = figura('sur', 0, NEGRO, dibujar)
            if par:
                base, sombra_ = par
                px = im.load()
                for y in range(im.height):
                    for x in range(im.width):
                        r, g, b, a = px[x, y]
                        if (r, g, b) in (HUESO, NARANJA, ORO): px[x, y] = base + (a,)
                        elif (r, g, b) == ORO_OSC: px[x, y] = sombra_ + (a,)
            im.save(A + f'Characters/Invitados/invitado_{mascara + 3 * color:02d}.png', optimize=True)

# ------------------------------------------------------------------ ícono de cronómetro (UI)
def cronometro():
    """Cronómetro de bolsillo (22 x 24 px), compañero de la llave en el HUD y en el reloj de capítulos."""
    W, H = 22, 24
    im = Image.new('RGBA', (W, H), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    oro, oro2 = (246, 200, 76, 255), (196, 140, 40, 255)
    plata, plata2, plata3 = (210, 214, 224, 255), (140, 146, 162, 255), (250, 250, 255, 255)
    esfera = (238, 230, 210, 255)
    d.rectangle([9, 0, 12, 1], fill=oro); d.rectangle([10, 2, 11, 3], fill=oro2)        # corona
    d.line([(16, 3), (18, 5)], fill=oro, width=2)                                       # botón lateral
    d.ellipse([1, 4, 20, 23], fill=plata2)
    d.ellipse([1, 4, 19, 22], fill=plata)
    d.arc([2, 5, 18, 21], 200, 260, fill=plata3)                                        # brillo del aro
    d.ellipse([4, 7, 17, 20], fill=esfera)
    for a in range(0, 360, 90):                                                         # marcas
        x = 10.5 + math.sin(math.radians(a)) * 5.2; y = 13.5 - math.cos(math.radians(a)) * 5.2
        d.point((round(x), round(y)), fill=(60, 50, 60, 255))
    d.line([(10, 13), (10, 9)], fill=(40, 30, 40, 255))                                 # manecilla
    d.line([(10, 13), (13, 15)], fill=(190, 30, 40, 255))                               # segundero rojo
    d.point((10, 13), fill=oro2)
    contorno(im).save(A + 'Props/cronometro.png', optimize=True)

if __name__ == '__main__':
    fondo(); relojes(); puerta(False); puerta(True); muebles(); invitados(); cronometro()
