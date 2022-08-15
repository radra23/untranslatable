# Untranslatable

**"When translators talk about untranslatable, they often reinforce the notion that each language has its own 'genius', an 'essence' that naturally sets it apart from all other languages and reflects something of the 'soul' of its culture or people." - Alexandra Jaffe**

---

An untranslatable word has no equivalent when translated into another language. The term describes the difficulty of achieving a perfect translation based on the notion that certain concepts and words are so interrelated that an exact translation becomes impossible. Meaning, however, can almost always be translated.

![alt text](/assets/image1.png "API diagram")

In this repo, we will instrument two APIs using [OpenTelemetry](https://opentelemetry.io/docs); one will be built in .NET and the other in Python. They are designed to have the same endpoints and purpose, returning untranslatable words that exist only in one language randomly or by language.

![alt text](/assets/image2.png "API diagram")

Different programming languages have different challenges, so we’ve selected examples in two technologies, Python and .NET, to demonstrate the consistency of OpenTelemetry across stacks. With OTel, we will perform tracing operations and use auto-instrumentation. Not all frameworks offer automatic instrumentation, but OpenTelemetry advises using it for those that do. Not only does it saves you time, but it provides a baseline for telemetry fast. Auto-instrumentation works by attaching an agent to the running application and extracting tracing data, much like a sidecar. If considering auto-instrumentation, remember that it lacks the flexibility of manual instrumentation and only captures basic signals.

[Jaeger](https://www.jaegertracing.io/) is a popular open-source distributed tracing tool initially built by teams at Uber, later being open-sourced and becoming part of the CNCF family. Works as a backend application for trace signals visualization and collection, allowing developers to visualize request traces through services in a system domain. Jaeger provides [instrumentation libraries](https://opentelemetry.io/docs/reference/specification/trace/sdk_exporters/jaeger/) built using OpenTracing standards that can offer a quick win on observing your application. In this repo, we will use the OTel exporter and OpenTelemetry's Jaeger exporter to send OTel traces to a Jaeger backend service.

<br>
<br>
<br>
<br>

---

## Contributors

<a href = "https://github.com/Tanu-N-Prabhu/Python/graphs/contributors">
<img src = "https://contrib.rocks/image?repo=radra23/untranslatable"/>
</a>
