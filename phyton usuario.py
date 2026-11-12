# ============================================================
#  LOGIN SIMPLE - Version pagina con Python
#  Un solo archivo: levanta un servidor local y abre el navegador.
#  Toda la logica esta en Python. No usa JavaScript.
#
#  Cuentas de ejemplo:
#    admin   / admin123   -> firulais / buenos aires / san martin
#    usuario / usuario123 -> luna / cordoba / belgrano
#
#  Para ejecutar:  python "phyton usuario.py"
# ============================================================

import html
import http.server
import json
import os
import webbrowser
from urllib.parse import parse_qs

CARPETA = os.path.dirname(os.path.abspath(__file__))
ARCHIVO = os.path.join(CARPETA, "usuarios.json")
PUERTO = 8000

estado = {"pendiente": None, "actual": None}


# ------------------------------------------------------------
#  DATOS
# ------------------------------------------------------------

def cargar_usuarios():
    if os.path.exists(ARCHIVO):
        with open(ARCHIVO, "r", encoding="utf-8") as archivo:
            return json.load(archivo)

    lista = [
        {
            "nombre": "Administrador",
            "usuario": "admin",
            "clave": "admin123",
            "rol": "admin",
            "respuestas": ["firulais", "buenos aires", "san martin"],
        },
        {
            "nombre": "Usuario comun",
            "usuario": "usuario",
            "clave": "usuario123",
            "rol": "comun",
            "respuestas": ["luna", "cordoba", "belgrano"],
        },
    ]
    guardar_usuarios(lista)
    return lista


def guardar_usuarios(lista):
    with open(ARCHIVO, "w", encoding="utf-8") as archivo:
        json.dump(lista, archivo, ensure_ascii=False, indent=2)


usuarios = cargar_usuarios()


# ------------------------------------------------------------
#  FUNCIONES DE APOYO
# ------------------------------------------------------------

def normalizar(texto):
    cambios = {
        "á": "a", "é": "e", "í": "i", "ó": "o", "ú": "u",
        "ü": "u", "ñ": "n",
    }
    texto = texto.lower().strip()
    salida = ""
    for letra in texto:
        salida = salida + cambios.get(letra, letra)
    return salida


def nombre_rol(rol):
    if rol == "admin":
        return "Administrador"
    return "Común"


def buscar(nombre):
    for u in usuarios:
        if u["usuario"] == nombre:
            return u
    return None


def contar_admins():
    total = 0
    for u in usuarios:
        if u["rol"] == "admin":
            total = total + 1
    return total


# ------------------------------------------------------------
#  PAGINAS HTML
# ------------------------------------------------------------

ESTILO = """
<style>
  body { max-width: 700px; margin: 40px auto; padding: 0 16px;
         font-family: Arial, sans-serif; text-align: center; }
</style>
"""


def pagina(contenido):
    return f"""<!DOCTYPE html>
<html lang="es">
<head>
<meta charset="utf-8">
<title>Login simple</title>
{ESTILO}
</head>
<body>
{contenido}
</body>
</html>"""


def lista_usuarios():
    texto = "<hr><h3>Usuarios del sistema</h3>"
    for u in usuarios:
        texto = texto + "<p><b>" + html.escape(u["usuario"]) + " (" + nombre_rol(u["rol"]) + ")</b><br>"
        texto = texto + "Clave: " + html.escape(u["clave"]) + "<br>"
        texto = texto + "Respuestas: " + html.escape(" / ".join(u["respuestas"])) + "</p>"
    return texto


def pagina_login(mensaje=""):
    formulario = """
<h2>Iniciar sesión</h2>
<form method="post" action="/login">
  Usuario: <input name="usuario"><br><br>
  Contraseña: <input name="clave" type="password"><br><br>
  <button>Ingresar</button>
</form>
<p>""" + mensaje + "</p>"
    return pagina(formulario + lista_usuarios())


def pagina_preguntas(mensaje=""):
    u = estado["pendiente"]
    nombre = ""
    if u is not None:
        nombre = html.escape(u["usuario"])

    formulario = f"""
<h2>Preguntas de seguridad</h2>
<p>Usuario: {nombre}</p>
<form method="post" action="/verificar">
  <p>1. ¿Nombre de tu primera mascota?<br><input name="r1"></p>
  <p>2. ¿Ciudad donde naciste?<br><input name="r2"></p>
  <p>3. ¿Nombre de tu escuela primaria?<br><input name="r3"></p>
  <button>Verificar e ingresar</button>
</form>
<p>{mensaje}</p>
"""
    return pagina(formulario)


def pagina_panel(mensaje=""):
    u = estado["actual"]
    if u is None:
        return pagina_login()

    contenido = f"""
<h2>Hola, {html.escape(u["nombre"])}</h2>
<p>Usuario: {html.escape(u["usuario"])} | Rol: {nombre_rol(u["rol"])}</p>
<p>{mensaje}</p>
"""

    if u["rol"] == "admin":
        contenido = contenido + "<hr><h3>Lista de usuarios</h3>"

        for otro in usuarios:
            contenido = contenido + "<p>" + html.escape(otro["usuario"]) + " (" + nombre_rol(otro["rol"]) + ") "

            if otro["usuario"] != u["usuario"]:
                contenido = contenido + (
                    '<form method="post" action="/eliminar" style="display:inline">'
                    '<input type="hidden" name="usuario" value="' + html.escape(otro["usuario"]) + '">'
                    '<button>Eliminar</button>'
                    '</form>'
                )

            contenido = contenido + "</p>"

        contenido = contenido + """
<hr>
<h3>Agregar usuario</h3>
<form method="post" action="/agregar">
  Nombre: <input name="nombre"><br><br>
  Usuario: <input name="usuario"><br><br>
  Contraseña: <input name="clave"><br><br>
  Rol: <select name="rol">
    <option value="comun">Común</option>
    <option value="admin">Administrador</option>
  </select><br><br>
  Respuesta 1: <input name="r1"><br><br>
  Respuesta 2: <input name="r2"><br><br>
  Respuesta 3: <input name="r3"><br><br>
  <button>Agregar</button>
</form>
"""

    contenido = contenido + '<hr><p><a href="/salir">Cerrar sesión</a></p>'
    return pagina(contenido)


# ------------------------------------------------------------
#  SERVIDOR
# ------------------------------------------------------------

def campo(datos, nombre):
    if nombre in datos:
        return datos[nombre][0].strip()
    return ""


class Manejador(http.server.BaseHTTPRequestHandler):

    def responder(self, contenido, codigo=200):
        datos = contenido.encode("utf-8")
        self.send_response(codigo)
        self.send_header("Content-Type", "text/html; charset=utf-8")
        self.send_header("Content-Length", str(len(datos)))
        self.end_headers()
        self.wfile.write(datos)

    def redirigir(self, destino):
        self.send_response(303)
        self.send_header("Location", destino)
        self.end_headers()

    def do_GET(self):
        if self.path == "/salir":
            estado["actual"] = None
            estado["pendiente"] = None
            self.redirigir("/")
        elif self.path == "/" or self.path == "/index.html":
            if estado["actual"] is None:
                self.responder(pagina_login())
            else:
                self.responder(pagina_panel())
        elif self.path == "/favicon.ico":
            self.responder("", 204)
        else:
            self.responder("<p>Página no encontrada.</p>", 404)

    def do_POST(self):
        largo = int(self.headers.get("Content-Length", "0"))
        cuerpo = self.rfile.read(largo).decode("utf-8")
        datos = parse_qs(cuerpo)

        if self.path == "/login":
            usuario = campo(datos, "usuario")
            clave = campo(datos, "clave")
            encontrado = buscar(usuario)

            if encontrado is None or encontrado["clave"] != clave:
                self.responder(pagina_login("Usuario o contraseña incorrectos."))
            else:
                estado["pendiente"] = encontrado
                self.responder(pagina_preguntas())

        elif self.path == "/verificar":
            u = estado["pendiente"]
            if u is None:
                self.redirigir("/")
                return

            r1 = normalizar(campo(datos, "r1"))
            r2 = normalizar(campo(datos, "r2"))
            r3 = normalizar(campo(datos, "r3"))

            if (r1 != normalizar(u["respuestas"][0]) or
                    r2 != normalizar(u["respuestas"][1]) or
                    r3 != normalizar(u["respuestas"][2])):
                self.responder(pagina_preguntas("Alguna respuesta es incorrecta."))
            else:
                estado["actual"] = u
                estado["pendiente"] = None
                self.responder(pagina_panel())

        elif self.path == "/agregar":
            u = estado["actual"]
            if u is None or u["rol"] != "admin":
                self.redirigir("/")
                return

            nombre = campo(datos, "nombre")
            usuario = campo(datos, "usuario")
            clave = campo(datos, "clave")
            rol = campo(datos, "rol")
            r1 = campo(datos, "r1")
            r2 = campo(datos, "r2")
            r3 = campo(datos, "r3")

            if (nombre == "" or usuario == "" or clave == "" or
                    r1 == "" or r2 == "" or r3 == ""):
                self.responder(pagina_panel("Completá todos los campos."))
            elif buscar(usuario) is not None:
                self.responder(pagina_panel("Ese usuario ya existe."))
            else:
                usuarios.append({
                    "nombre": nombre,
                    "usuario": usuario,
                    "clave": clave,
                    "rol": rol,
                    "respuestas": [r1, r2, r3],
                })
                guardar_usuarios(usuarios)
                self.responder(pagina_panel("Usuario agregado."))

        elif self.path == "/eliminar":
            u = estado["actual"]
            if u is None or u["rol"] != "admin":
                self.redirigir("/")
                return

            nombre = campo(datos, "usuario")
            objetivo = buscar(nombre)

            if objetivo is None:
                self.responder(pagina_panel("Ese usuario no existe."))
            elif objetivo["usuario"] == u["usuario"]:
                self.responder(pagina_panel("No podés eliminar tu propia cuenta."))
            elif objetivo["rol"] == "admin" and contar_admins() == 1:
                self.responder(pagina_panel("No se puede eliminar al último administrador."))
            else:
                usuarios.remove(objetivo)
                guardar_usuarios(usuarios)
                self.responder(pagina_panel("Usuario eliminado."))

        else:
            self.responder("<p>Página no encontrada.</p>", 404)


def main():
    try:
        servidor = http.server.HTTPServer(("127.0.0.1", PUERTO), Manejador)
    except OSError:
        print("El puerto " + str(PUERTO) + " está ocupado.")
        print("Cambiá el valor de PUERTO al principio del archivo y volvé a intentar.")
        return

    direccion = "http://127.0.0.1:" + str(PUERTO)
    print("Servidor iniciado en " + direccion)
    print("Para cerrar, apretá Ctrl+C en esta ventana.")
    webbrowser.open(direccion)
    servidor.serve_forever()


if __name__ == "__main__":
    main()
