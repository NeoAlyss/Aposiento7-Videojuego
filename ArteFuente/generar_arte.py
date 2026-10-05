"""Genera el arte procedural de los menús: fondo de vitrales desenfocado y la vela.
Uso: python3 ArteFuente/generar_arte.py  (requiere Pillow y numpy)"""
import math, random
import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageChops

RAIZ = 'Assets/Art/'

# ---------------------------------------------------------------- fondo
def ventana_mask(W, H, cx, base_y, ancho, alto, ss=1):
    """Ventana ojival: rectángulo + arco apuntado, con parteluz, travesaños y un rosetón simple."""
    m = Image.new('L', (W, H), 0); d = ImageDraw.Draw(m)
    x0, x1 = cx - ancho / 2, cx + ancho / 2
    y_arco = base_y - alto + ancho * 0.9          # donde empieza el arco
    d.rectangle([x0, y_arco, x1, base_y], fill=255)
    # arco apuntado = intersección de dos círculos de radio = ancho
    arco = Image.new('L', (W, H), 0); da = ImageDraw.Draw(arco)
    c1 = Image.new('L', (W, H), 0); ImageDraw.Draw(c1).ellipse([x0, y_arco - ancho, x0 + 2 * ancho, y_arco + ancho], fill=255)
    c2 = Image.new('L', (W, H), 0); ImageDraw.Draw(c2).ellipse([x1 - 2 * ancho, y_arco - ancho, x1, y_arco + ancho], fill=255)
    arco = ImageChops.multiply(c1, c2)
    ImageDraw.Draw(arco).rectangle([0, y_arco, W, H], fill=0)
    m = ImageChops.lighter(m, arco)
    # plomos (líneas oscuras)
    d = ImageDraw.Draw(m); g = max(2, int(ancho * 0.045))
    d.line([cx, y_arco - ancho * 0.25, cx, base_y], fill=0, width=g)
    n = int((base_y - y_arco) / (ancho * 0.42))
    for i in range(1, n + 1):
        y = base_y - i * (base_y - y_arco) / (n + 0.3)
        d.line([x0, y, x1, y], fill=0, width=max(1, g // 2))
    r = ancho * 0.2; cy = y_arco - ancho * 0.36
    d.ellipse([cx - r, cy - r, cx + r, cy + r], outline=0, width=g)
    d.line([x0, y_arco, x1, y_arco], fill=0, width=g)
    return m

def generar_fondo():
    W, H = 1920, 1080
    rng = random.Random(7)
    ventanas = [(W * 0.385, 0.66 * H, 150, 470), (W * 0.5, 0.66 * H, 170, 560), (W * 0.615, 0.66 * H, 150, 470)]
    mask = Image.new('L', (W, H), 0)
    for cx, by, a, al in ventanas:
        mask = ImageChops.lighter(mask, ventana_mask(W, H, cx, by, a, al))
    m = np.asarray(mask, dtype=np.float32) / 255.0

    yy, xx = np.mgrid[0:H, 0:W].astype(np.float32)
    # base: casi negro con un tinte granate, algo más claro hacia el centro
    d = np.hypot((xx - W / 2) / W, (yy - H * 0.5) / H)
    base = np.clip(1 - d * 1.9, 0, 1) ** 1.6
    img = np.zeros((H, W, 3), np.float32)
    img[..., 0] = 0.035 + 0.10 * base; img[..., 1] = 0.012 + 0.022 * base; img[..., 2] = 0.016 + 0.018 * base

    # resplandor amplio alrededor de las ventanas
    halo = np.asarray(mask.filter(ImageFilter.GaussianBlur(110)), np.float32) / 255.0
    halo2 = np.asarray(mask.filter(ImageFilter.GaussianBlur(40)), np.float32) / 255.0
    img += np.stack([halo * 1.25 + halo2 * 0.6, halo * 0.50 + halo2 * 0.33, halo * 0.10 + halo2 * 0.07], -1)

    # rayos: cada ventana proyecta un haz que se abre hacia abajo y hacia los lados
    rayos = np.zeros((H, W), np.float32)
    for (cx, by, a, al), incl in zip(ventanas, (-0.55, 0.0, 0.55)):
        t = np.clip((yy - (by - al * 0.55)) / (H * 0.75), 0, 1)
        centro = cx + incl * (yy - (by - al * 0.5))
        ancho = a * 0.55 + t * 240
        perfil = np.exp(-((xx - centro) / ancho) ** 2)
        rayos += perfil * (1 - t) ** 1.2 * (t > 0) * 0.34
    img += np.stack([rayos * 1.0, rayos * 0.42, rayos * 0.08], -1)

    # reflejo cálido en el suelo
    suelo = np.exp(-((xx - W / 2) / (W * 0.20)) ** 2) * np.exp(-((yy - H * 0.86) / (H * 0.08)) ** 2) * 0.30
    img += np.stack([suelo, suelo * 0.35, suelo * 0.05], -1)

    # vidrio: amarillo crema muy luminoso, algo más anaranjado en los bordes
    borde = np.asarray(mask.filter(ImageFilter.GaussianBlur(9)), np.float32) / 255.0
    vidrio = np.stack([np.ones_like(m), 0.80 + 0.17 * borde, 0.42 + 0.40 * borde ** 3], -1)
    img = img * (1 - m[..., None]) + vidrio * m[..., None]

    # viñeta
    v = np.clip(1.08 - np.hypot((xx - W / 2) / (W * 0.62), (yy - H / 2) / (H * 0.72)) ** 2.4 * 0.75, 0.12, 1)
    img *= v[..., None]

    out = Image.fromarray((np.clip(img, 0, 1) * 255).astype(np.uint8), 'RGB')
    # motas de polvo iluminadas
    polvo = Image.new('RGB', (W, H), 0); dp = ImageDraw.Draw(polvo)
    for _ in range(90):
        x = rng.gauss(W / 2, W * 0.17); y = rng.uniform(H * 0.25, H * 0.93); r = rng.uniform(1.5, 5)
        k = rng.uniform(0.25, 0.9)
        dp.ellipse([x - r, y - r, x + r, y + r], fill=(int(255 * k), int(170 * k), int(60 * k)))
    out = ImageChops.add(out, polvo.filter(ImageFilter.GaussianBlur(2)))
    # desenfoque suave para que no compita con el reloj ni con el menú
    out = out.filter(ImageFilter.GaussianBlur(9))
    out.save(RAIZ + 'Backgrounds/fondo_vitrales.png', optimize=True)

# ---------------------------------------------------------------- vela
def generar_vela():
    ss = 4
    # cuerpo: 256 x 640, la cera se pierde en la oscuridad hacia abajo
    W, H = 256 * ss, 640 * ss
    m = Image.new('L', (W, H), 0); d = ImageDraw.Draw(m)
    cx = W // 2; a = 34 * ss; top = 46 * ss
    d.rectangle([cx - a, top + 10 * ss, cx + a, H], fill=255)
    d.ellipse([cx - a, top, cx + a, top + 22 * ss], fill=255)                 # borde superior
    d.ellipse([cx + a - 14 * ss, top + 2 * ss, cx + a + 6 * ss, top + 30 * ss], fill=255)   # labio derretido
    d.ellipse([cx - a - 3 * ss, top + 6 * ss, cx - a + 14 * ss, top + 44 * ss], fill=255)
    m = m.filter(ImageFilter.GaussianBlur(1.5 * ss)).point(lambda v: 255 if v > 128 else 0).filter(ImageFilter.GaussianBlur(0.8 * ss))
    yy, xx = np.mgrid[0:H, 0:W].astype(np.float32)
    u = (xx - cx) / a                                    # -1..1 a lo ancho
    cil = np.clip(1 - np.abs(u) ** 2.2 * 0.75, 0, 1)      # volumen cilíndrico
    luz = np.exp(-(np.clip(yy - top, 0, None) / (150 * ss)) ** 1.4)       # la llama ilumina solo la parte alta
    r = (0.16 + 0.84 * luz) * (0.55 + 0.45 * cil); g = (0.02 + 0.13 * luz) * cil; b = (0.03 + 0.10 * luz) * cil
    # cera translúcida junto a la llama
    brillo = np.exp(-((yy - top - 6 * ss) / (26 * ss)) ** 2) * np.exp(-(u / 0.8) ** 2)
    r += brillo * 0.25; g += brillo * 0.30; b += brillo * 0.12
    alpha = (np.asarray(m, np.float32) / 255) * np.clip(1.15 - (np.clip(yy - top, 0, None) / (H - top)) ** 1.6 * 1.15, 0, 1)
    rgba = np.stack([r, g, b, alpha], -1)
    cuerpo = Image.fromarray((np.clip(rgba, 0, 1) * 255).astype(np.uint8), 'RGBA')
    dm = ImageDraw.Draw(cuerpo)
    dm.line([cx, top + 8 * ss, cx - 1 * ss, top - 12 * ss], fill=(22, 10, 8, 255), width=3 * ss)     # mecha
    cuerpo.resize((256, 640), Image.LANCZOS).save(RAIZ + 'Candle/vela_cuerpo.png', optimize=True)

    # llama: 128 x 256, con la base en el borde inferior
    W, H = 128 * ss, 256 * ss
    yy, xx = np.mgrid[0:H, 0:W].astype(np.float32)
    t = 1 - (yy - 10 * ss) / (H - 20 * ss)                 # 0 abajo, 1 en la punta
    t = np.clip(t, 0, 1)
    ancho = 30 * ss * np.clip(np.sin(np.clip(t * 1.05, 0, 1) ** 0.62 * math.pi), 0, 1) ** 0.9 * (1 - t * 0.35) + 0.5
    dx = np.abs(xx - W / 2) / ancho
    forma = np.clip(1.25 - dx ** 2.2 * 1.25, 0, 1) * (t > 0) * (t < 1)
    nucleo = np.clip(1.4 - dx ** 2 * 3.2, 0, 1) * np.clip(t * 3, 0, 1)
    r = np.ones_like(forma); g = 0.55 + 0.42 * nucleo * (0.5 + 0.5 * t); b = 0.10 + 0.62 * nucleo * t
    base_azul = np.clip(1 - t * 9, 0, 1)                   # la base de la llama es más fría y tenue
    r = r * (1 - base_azul * 0.55); g = g * (1 - base_azul * 0.2); b = b + base_azul * 0.45
    alpha = forma ** 0.8 * (1 - base_azul * 0.45)
    llama = Image.fromarray((np.clip(np.stack([r, g, b, alpha], -1), 0, 1) * 255).astype(np.uint8), 'RGBA')
    llama = llama.filter(ImageFilter.GaussianBlur(1.2 * ss)).resize((128, 256), Image.LANCZOS)
    llama.save(RAIZ + 'Candle/vela_llama.png', optimize=True)

    # halo: resplandor radial blanco (se tiñe en Unity)
    S = 512; yy, xx = np.mgrid[0:S, 0:S].astype(np.float32)
    d = np.hypot(xx - S / 2, yy - S / 2) / (S / 2)
    a = np.clip(1 - d, 0, 1) ** 2.4
    halo = np.stack([np.ones_like(a)] * 3 + [a], -1)
    Image.fromarray((halo * 255).astype(np.uint8), 'RGBA').save(RAIZ + 'Candle/vela_halo.png', optimize=True)

if __name__ == '__main__':
    generar_fondo(); generar_vela()
