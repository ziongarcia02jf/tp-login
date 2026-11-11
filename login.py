# =============================================================
# TP: Identificación y Autenticación — versión SEGURA
# Formato de cada línea del archivo:
#   "nombre;hash_contraseña;pregunta;hash_respuesta;tipo"
#    0       1              2        3             4
#
# Medidas de seguridad aplicadas:
# 1) La contraseña NO se guarda en texto plano: se guarda su
#    HASH SHA-256. Aunque alguien abra el archivo, no puede ver
#    ni recuperar la clave original.
# 2) getpass: al tipear la contraseña no se ve en la pantalla.
# 3) Límite de intentos: a los 3 fallos seguidos se espera 10
#    segundos (dificulta el ataque de fuerza bruta).
# 4) Mensaje genérico en el login: no revela si el usuario existe
#    o si solo falló la contraseña (evita enumerar usuarios).
# 5) Validaciones: mínimo 6 caracteres y prohibido el «;»
#    (que es el separador del formato del archivo).
# 6) Recuperar contraseña = crear una NUEVA. Nunca se muestra la
#    vieja: con el hash guardado ni el propio sistema puede leerla.
# =============================================================

import hashlib
import sys
import time
from getpass import getpass

ARCHIVO = 'usuarios.txt'
INTENTOS_MAX = 3
ESPERA = 10          # segundos de espera por fuerza bruta

fallos = 0           # contador de intentos fallidos del login


def hashear(texto):
    """Devuelve el hash SHA-256 del texto (siempre 64 caracteres).
    Es un camino de ida: del hash no se puede volver al texto."""
    return hashlib.sha256(texto.encode('utf-8')).hexdigest()


def leer_contrasenia(mensaje):
    """getpass oculta lo tipeado en un terminal real. Si la entrada
    no es interactiva (ej: prueba automática) usa input() común."""
    if sys.stdin.isatty():
        return getpass(mensaje)
    return input(mensaje)


def cargar_usuarios():
    """open(..., 'r') = lectura. Devuelve cada línea cortada por ';'
    (igual que el split del pizarrón)."""
    lista = []
    try:
        archivo = open(ARCHIVO, 'r', encoding='utf-8')
        for linea in archivo.readlines():
            linea = linea.strip()
            if linea != '':
                lista.append(linea.split(';'))
        archivo.close()
    except FileNotFoundError:
        pass
    return lista


def guardar_usuario(nombre, hash_pass, pregunta, hash_resp, tipo):
    """open(..., 'a') = escritura/agregar (agrega al final)."""
    archivo = open(ARCHIVO, 'a', encoding='utf-8')
    archivo.write(nombre + ';' + hash_pass + ';' + pregunta + ';'
                  + hash_resp + ';' + tipo + '\n')
    archivo.close()


def actualizar_contrasenia(nombre, nuevo_hash):
    """open(..., 'w') = escritura: reescribe el archivo completo
    cambiando SOLO la línea del usuario indicado."""
    usuarios = cargar_usuarios()
    for datos in usuarios:
        if datos[0].lower() == nombre.lower():
            datos[1] = nuevo_hash
            break
    archivo = open(ARCHIVO, 'w', encoding='utf-8')
    for datos in usuarios:
        archivo.write(';'.join(datos) + '\n')
    archivo.close()


def registrar():
    """Creación de usuario (Admin o Común)."""
    print('--- REGISTRARSE ---')
    nombre = input('Usuario: ').strip()

    if not nombre:
        print('El usuario no puede estar vacío.\n')
        return
    if ';' in nombre:
        print('El usuario no puede contener «;» (separa los campos).\n')
        return

    # usuarios duplicados, sin importar mayúsculas/minúsculas
    for datos in cargar_usuarios():
        if datos[0].lower() == nombre.lower():
            print('Ese usuario ya existe. Elegí otro.\n')
            return

    contrasenia = leer_contrasenia('Contraseña: ')
    if len(contrasenia) < 6:
        print('La contraseña debe tener al menos 6 caracteres.\n')
        return
    if ';' in contrasenia:
        print('La contraseña no puede contener «;».\n')
        return
    repetida = leer_contrasenia('Repetí la contraseña: ')
    if contrasenia != repetida:
        print('Las contraseñas no coinciden.\n')
        return

    pregunta = input('Pregunta de seguridad: ').strip()
    respuesta = input('Respuesta (confirmación): ').strip()
    if not pregunta or not respuesta:
        print('Falta la pregunta o la respuesta.\n')
        return
    if ';' in pregunta or ';' in respuesta:
        print('No se permite el carácter «;».\n')
        return

    tipo = input('Tipo (admin/comun): ').strip().lower()
    if tipo != 'admin':
        tipo = 'comun'

    # se guardan los HASH, nunca el texto plano
    guardar_usuario(nombre, hashear(contrasenia), pregunta,
                    hashear(respuesta), tipo)
    print('¡Usuario', nombre, 'creado con éxito!\n')


def iniciar_sesion():
    """Identificación (quién soy) + Autenticación (confirmar)."""
    global fallos
    print('--- INICIAR SESIÓN ---')
    nombre = input('Usuario: ').strip()
    contrasenia = leer_contrasenia('Contraseña: ')

    for datos in cargar_usuarios():
        if datos[0].lower() == nombre.lower() \
                and datos[1] == hashear(contrasenia):
            fallos = 0   # acertó: se reinicia el contador
            print('¡Bienvenido', datos[0] + '!', '(tipo:', datos[4] + ')\n')
            return

    fallos += 1
    # mensaje GENÉRICO: no aclara si el usuario existe o no
    print('Usuario o contraseña incorrectos.')
    if fallos >= INTENTOS_MAX:
        print('Demasiados intentos. Esperá', ESPERA, 'segundos...')
        time.sleep(ESPERA)
        fallos = 0
    print()


def recuperar():
    """Autenticación por 'algo que sé' + cambio de contraseña.
    Como se guarda el hash, la vieja NO se puede mostrar: se crea
    una nueva reescribiendo el archivo con modo 'w'."""
    print('--- RECUPERAR CONTRASEÑA ---')
    nombre = input('Usuario: ').strip()

    encontrada = None
    for datos in cargar_usuarios():
        if datos[0].lower() == nombre.lower():
            encontrada = datos
            break

    if encontrada is None:
        # mensaje genérico: no revela si el usuario existe
        print('No se pudo recuperar la contraseña con esos datos.\n')
        return

    print('Pregunta de seguridad:', encontrada[2])
    respuesta = input('Respuesta: ').strip()

    if hashear(respuesta) != encontrada[3]:
        print('Respuesta incorrecta.\n')
        return

    # respuesta correcta: se permite definir una contraseña nueva
    nueva = leer_contrasenia('Nueva contraseña: ')
    if len(nueva) < 6 or ';' in nueva:
        print('La nueva contraseña no cumple los requisitos.\n')
        return
    repetida = leer_contrasenia('Repetí la nueva contraseña: ')
    if nueva != repetida:
        print('Las contraseñas no coinciden.\n')
        return

    actualizar_contrasenia(encontrada[0], hashear(nueva))
    print('¡Contraseña actualizada! Ya podés iniciar sesión.\n')


def menu():
    while True:
        print('===== IDENTIFICACIÓN Y AUTENTICACIÓN =====')
        print('1) Registrarse')
        print('2) Iniciar sesión')
        print('3) Recuperar contraseña')
        print('4) Salir')
        opcion = input('Elegí una opción: ')

        if opcion == '1':
            registrar()
        elif opcion == '2':
            iniciar_sesion()
        elif opcion == '3':
            recuperar()
        elif opcion == '4':
            print('¡Chau!')
            break
        else:
            print('Opción inválida.\n')


menu()
