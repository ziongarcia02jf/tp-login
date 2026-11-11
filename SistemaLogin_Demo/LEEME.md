# Sistema Login — Común y Administrador

Aplicación de escritorio simple hecha en **C# con Windows Forms** para **Visual Studio 2012** y **.NET Framework 4.5**. Sirve como demo escolar de un sistema de identificación y autenticación con dos roles.

## Funcionalidad

- **Login común:** usuario, contraseña y 3 preguntas de seguridad.
- **Login de administrador:** además de ingresar, puede administrar las cuentas.
- **Panel de administración:**
  - Ver todos los usuarios registrados.
  - Agregar usuarios nuevos (rol Usuario o Administrador).
  - Eliminar usuarios.
  - No permite eliminar la propia cuenta ni al último administrador.
- **Panel de usuario común:** muestra los datos de la cuenta y el último acceso.
- **Sin recuperación de contraseña** (a pedido).
- **3 preguntas fijas de seguridad** para completar la autenticación:
  1. ¿Nombre de tu primera mascota?
  2. ¿Ciudad donde naciste?
  3. ¿Nombre de tu escuela primaria?

## Cuentas de demostración

La primera vez que se ejecuta, el sistema crea automáticamente dos cuentas:

| Rol | Usuario | Contraseña | Respuestas de seguridad |
|---|---|---|---|
| Administrador | `admin` | `admin123` | `firulais` / `buenos aires` / `san martin` |
| Usuario común | `usuario` | `usuario123` | `luna` / `cordoba` / `belgrano` |

Las respuestas **no distinguen mayúsculas, minúsculas ni acentos**.

> Para la demo escolar podés usar estas cuentas o crear usuarios nuevos desde el panel del administrador.

## Cómo abrir y ejecutar

1. Abrir `SistemaLogin.sln` con **Visual Studio 2012**.
2. Verificar que el proyecto use **.NET Framework 4.5**.
3. Presionar **F5** o **Iniciar**.
4. Ingresar con una de las cuentas de demostración.
5. Responder las 3 preguntas de seguridad.

También se puede compilar desde la consola con el compilador de .NET Framework:

```text
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /out:SistemaLogin.exe *.cs
```

## Dónde se guardan los datos

```text
%AppData%\SistemaLogin\usuarios.xml
```

- El archivo se crea automáticamente en el primer inicio.
- Para **reiniciar la demo** (borrar todos los usuarios y volver a las cuentas de ejemplo), cerrar la aplicación y eliminar la carpeta `%AppData%\SistemaLogin`.
- Se guarda una copia de respaldo `usuarios.xml.bak`.

## Seguridad

- Las contraseñas **no se guardan en texto plano**.
- Se usa **PBKDF2** (`Rfc2898DeriveBytes`) con **10.000 iteraciones**, `salt` aleatorio por usuario y hash de 32 bytes.
- Las respuestas de seguridad también se guardan con hash y `salt`.
- La comparación de hashes se hace en tiempo constante.
- Las cuentas de demostración son solo para mostrar el funcionamiento. En un sistema real habría que cambiar las claves, usar HTTPS/base de datos y agregar bloqueo por intentos fallidos.

## Estructura del proyecto

```text
SistemaLogin/
├── SistemaLogin.sln
└── SistemaLogin/
    ├── Program.cs
    ├── App.config
    ├── Data/
    │   ├── UserDatabase.cs
    │   ├── UserRecord.cs
    │   └── UserStore.cs
    ├── Forms/
    │   ├── LoginForm.cs
    │   ├── VerificationForm.cs
    │   ├── MainForm.cs
    │   └── UserEditorForm.cs
    ├── Security/
    │   ├── PasswordHasher.cs
    │   └── SecurityQuestions.cs
    └── Properties/
        └── AssemblyInfo.cs
```

## Notas

- La interfaz usa controles estándar de Windows Forms (`Label`, `TextBox`, `Button`, `ListBox`, `ComboBox`) y está armada por código para que compile sin archivos `.Designer.cs` ni `.resx`.
- Por eso, si abrís un formulario en el diseñador de Visual Studio, se va a ver vacío; la aplicación funciona igual.
- Las ventanas tienen tamaño fijo para mantener el diseño simple.
- No se modificó el proyecto **GasGuard**.
