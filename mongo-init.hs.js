db = db.getSiblingDB('BiblioMateLogs');

db.logEntries.insertOne({
    userId: 1,
    type: "INFO",
    message: "Document initial inséré automatiquement depuis mongo-init.js",
    sentAt: new Date()
});
