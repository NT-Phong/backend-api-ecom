# Integration inventory

Current Infrastructure top-level integration areas include:

- Caching and distributed locking.
- Event, EventBus, Messaging, Metrics, and Telemetry.
- IoT and device processing.
- Security and seeding.
- Camera, AI, FCM, notification, SMS, storage, warehouse, and document services.

The project files currently declare external package references for Azure Event Hubs, Azure Service Bus, Azure Blob Storage, Firebase, MQTT, Redis/SignalR, image processing, spreadsheet generation, and related integrations.

These modules are outside the first starter release. They must be removed together with their DI registration, interfaces, options binding, workers, controllers/hubs, configuration, and package references.
