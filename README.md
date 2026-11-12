# TP: Identificación y Autenticación

Sistema de usuarios desarrollado en **tres versiones** del mismo trabajo práctico:
una de **terminal en Python**, otra **web en JavaScript/HTML** y una
**de escritorio en C# con Windows Forms**.

Permite tres operaciones:

1. **Registrarse** — creación de usuarios (Admin o Común)
2. **Iniciar sesión** — identificación + autenticación
3. **Recuperar contraseña** — autenticación por pregunta de seguridad

## Conceptos aplicados

- **Identificación:** el usuario declara quién es (el nombre).
- **Autenticación:** demuestra que es él (algo que sabe: su contraseña
  o la respuesta a su pregunta de seguridad).

## Formato del archivo de usuarios

Cada usuario se guarda en una línea, con los campos separados por `;`
(formato dado en clase):

```
nombre;contraseña;pregunta;confirmación;tipo
  0        1         2         3       4
```

| Posición | Campo |
|----------|-------|
| 0 | Nombre de usuario |
| 1 | Contraseña (guardada como **hash SHA-256**) |
| 2 | Pregunta de seguridad |
| 3 | Respuesta/confirmación (guardada como **hash SHA-256**) |
| 4 | Tipo de usuario (admin / comun) |

- **Python:** archivo de texto `usuarios.txt`, leído con `open()` +
  `readlines()`, agregado con modo `'a'` y reescrito con modo `'w'`.
- **JavaScript:** el navegador no maneja archivos de texto, por eso se
  usa `localStorage`, el equivalente del navegador (la consola inferior
  de la página muestra el "archivo" en vivo).

## Seguridad implementada

| Problema | Solución |
|----------|----------|
| Contraseñas en texto plano | Se guarda su **hash SHA-256** (`hashlib` en Python, `crypto.subtle` en el navegador): del hash no se puede volver al texto |
| Ataque de fuerza bruta | Límite de intentos: a los 3 fallos, bloqueo de 15 s (web) / espera de 10 s (Python) |
| Enumeración de usuarios | Mensajes genéricos: "Usuario o contraseña incorrectos", sin revelar si el usuario existe |
| Contraseña visible al tipear | `getpass` en Python (no se muestra en pantalla) |
| Recuperación que muestra la clave original | **Reset:** si la respuesta es correcta, se define una contraseña nueva (la vieja nunca se muestra, con el hash ni el sistema puede leerla) |
| Campos que rompen el formato | Prohibido el carácter `;` (es el separador) |
| Contraseñas demasiado débiles | Mínimo 6 caracteres + confirmación doble |
| Problemas de codificación con ñ/acentos | Archivos abiertos con `encoding='utf-8'` |

## Cómo ejecutarlo

### Versión Python (terminal)

```
python login.py
```

Requiere Python 3. No usa librerías externas (solo `hashlib`,
`getpass`, `time` y `sys` de la librería estándar).

### Versión web

Abrir `login.html` con doble clic (funciona en Chrome, Edge y Firefox,
también sin internet). Incluye medidor de fuerza de contraseña,
bloqueo por intentos fallidos y una consola que muestra el
"archivo" de usuarios en tiempo real.

### Versión web simple

Abrir `phyton usuario.html` con doble clic. Es una versión corta y fácil
de estudiar: login con usuario y contraseña, 3 preguntas de seguridad,
rol **administrador** o **común**, y el administrador puede agregar y
eliminar usuarios. Al lado del login muestra la lista de usuarios con
su clave y sus respuestas.

### Versión Python con página local

Ejecutar:

```
python "phyton usuario.py"
```

El archivo levanta un servidor local con `http.server` (librería estándar)
y abre el navegador solo. La página se ve en el navegador, pero **toda la
lógica está en Python**: login, 3 preguntas, roles, agregar y eliminar
usuarios. No usa JavaScript. Los usuarios se guardan en `usuarios.json`.

Es la versión **100% Python** del TP: no hay JavaScript ni lógica en el
navegador; el navegador solo muestra el HTML que genera Python.

Requiere Python 3 instalado. Para cerrar el servidor, apretar `Ctrl+C`
en la ventana de la consola.

### Versión C# (Windows Forms)

La carpeta `SistemaLogin_Demo` contiene una aplicación de escritorio
para **Visual Studio 2012 / .NET Framework 4.5**.

- Login de **usuario común** y **administrador**.
- Autenticación con usuario, contraseña y **3 preguntas de seguridad**.
- El administrador puede **agregar y eliminar usuarios**.
- No incluye recuperación de contraseña.
- Contraseñas y respuestas guardadas con **PBKDF2 + salt**.

Para probarla, ejecutar `SistemaLogin_Demo/SistemaLogin.exe`.
Para abrir el código, abrir `SistemaLogin_Demo/Codigo/SistemaLogin.sln`.

## Estructura

```
login.py            → versión de terminal (Python)
login.html          → versión web (HTML + CSS + JavaScript, todo en un archivo)
phyton usuario.html → versión web simple (login, roles y usuarios)
phyton usuario.py   → versión Python con página local (servidor http.server)
SistemaLogin_Demo/  → versión de escritorio (C# / Windows Forms)
```
