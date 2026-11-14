# Cómo correr los tests en Visual Studio

Equivalente a lo que viste en IntelliJ (panel de tests verdes). Stack: **xUnit + Moq + FluentAssertions**.

## 1. Abrir la solución

1. Abrí **Visual Studio 2022** (o 2026 si tenés).
2. **File → Open → Project/Solution…**
3. Elegí:

```text
d:\UPC CICLO 8\IOT 8\safeplant-web-backend\safeplant-web-backend.slnx
```

4. En el Explorador de soluciones deberías ver la carpeta/proyecto **`SafePlant.UnitTests`**.

## 2. Restaurar y compilar

- Clic derecho en la solución → **Restore NuGet Packages**
- Menú **Build → Build Solution** (`Ctrl+Shift+B`)

## 3. Test Explorer (como el Run de IntelliJ)

1. Menú **Test → Test Explorer** (o `Ctrl+E, T`).
2. Pulsá el botón **Run All** (doble play).
3. Deberías ver todos los tests en verde, agrupados por clase:
   - `ExposurePolicyTests`
   - `ActuatorPolicyTests`
   - `UserAccountTests`
   - `SessionTests`
   - `SignInServiceTests`
   - `OverrideServiceTests`
   - `ThresholdServiceTests`
   - `AreaServiceTests`

También podés hacer clic derecho en un test → **Run** / **Debug**.

## 4. Alternativa desde terminal (Cursor o PowerShell)

Desde la raíz del repo:

```powershell
dotnet test "tests\SafePlant.UnitTests\SafePlant.UnitTests.csproj"
```

O toda la solución:

```powershell
dotnet test "safeplant-web-backend.slnx"
```

## 5. Qué cubren estos tests

| Estilo Java (tu captura) | Aquí en C# |
| --- | --- |
| Mockito `when(...).thenReturn` | Moq `Setup(...).Returns` |
| MockMvc + assert status/JSON | Tests de **servicios de Application** + `AppException.Code` |
| Reglas de negocio | Tests de **Domain** (ExposurePolicy, ActuatorPolicy, roles) |

No necesitan Postgres ni Azure: son unit tests con mocks.

## 6. Si Test Explorer está vacío

1. Compilá de nuevo el proyecto de tests.
2. En Test Explorer → icono de engranaje → asegurá que el adaptador **xUnit** esté habilitado.
3. Confirmá que el SDK es **.NET 10** (`dotnet --version`).
