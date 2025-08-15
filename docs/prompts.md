# Prompts utilizados

Listado breve de prompts de GenAI usados durante el desarrollo.  
Incluye contexto, prompt y resultado esperado.

---

## Test E2E con ETag
### Herramienta: Chat Gpt

**Contexto:**  
Validar que el endpoint `/api/v1/products` soporte cache condicional (If-None-Match → 304).

**Prompt:**  
```

Realiza un test en xUnit para API .NET 8. La ruta es /api/v1/products?ids=<id>, tomando en cuenta principios de buenas practicas de codigo, eficiencia y escalabilidad.
Realiza:
GET inicial devuelva 200 y ETag.
Realiza lo mismo pero con GET con If-None-Match igual al ETag para obtener 304 sin body.

```

**Resultado esperado:**  
Test que obtiene un ETag y verifica respuesta 304 sin contenido.

---

## Middleware de logging
### Herramienta: Chat Gpt

**Contexto:**  
Mejorar trazabilidad y medir latencia por request.

**Prompt:**  
```

Para el program.cs de un proyecto de un ms .NET Core 8, indicame como configurar un middleware que loguee inicio y fin con status y tiempo para loguear en base a eficiencia, escalabilidad y buenas practicas de codigo.

```

**Resultado esperado:**  
Middleware funcional integrado en `Program.cs` que añade correlación y logging.

---

## 3. README y run.md
### Herramienta: Copilot Agent

**Contexto:**  
Documentar arquitectura, endpoints, manejo de errores, caching y tests.

**Prompt:**  
```

Analiza todo el proyecto, archivo por archivo y en el archivo README.md genera un template en base a tu analisis completo. Detalla arquitectura, endpoints, errores, caching, logging y el flujo de proceso del microservicio.

```

**Resultado esperado:**  
Plantilla de README y run.md ajustadas manualmente al código real.

---

## 4. Tests unitarios
### Herramienta: Copilot Agent

**Contexto:**  
Generar archivos de test como base.

**Prompt:**  
```

Analiza todo el proyecto, archivo por archivo y en base al mismo, genera archivos de test xUnit dentro de los proyectos correspondientes, teniendo siempre en cuenta de cubrir en un 100% todos los posibles casos de prueba.

```

**Resultado esperado:**  
Tests con Moq que cubren todos los escenarios.

**Particularmente en este caso, una vez generado los archivos, de forma reiterada le solicite a CopilotAgent que re-analice para volver a generar archivos necesarios ya que nunca los terminaba por completo. Una vez terminado a mi criterio, se procedio a mitigar el codigo xUnit y refactorizar lo necesario**

---

## 5. Propuesta de Loggers en archivos 
### Herramienta: Copilot Agent

**Contexto:**  
Consultar ubicacion de loggers en el codigo de cada archivo de la arquitectura 

**Prompt:**  
```

Revisa el archivo e indicame donde crees eficiente, en terminos de buenas practicas de codigo, eficiencia y escalabilidad, realizar logs en el codigo y sobre que datos especificos.

```

**Resultado esperado:**  
Base de logs ubicados estrategicamente sobre todo el flujo del ms.

---