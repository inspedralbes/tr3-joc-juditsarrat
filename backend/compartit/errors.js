
// Classes d'error personalitzades per a una millor gestió d'excepcions.

class NotFoundError extends Error {
    constructor(message) {
        super(message || "Recurs no trobat.");
        this.name = "NotFoundError";
        this.statusCode = 404;
    }
}

class ValidationError extends Error {
    constructor(message) {
        super(message || "Dades d'entrada no vàlides.");
        this.name = "ValidationError";
        this.statusCode = 400;
    }
}

class UnauthorizedError extends Error {
    constructor(message) {
        super(message || "Accés no autoritzat.");
        this.name = "UnauthorizedError";
        this.statusCode = 401;
    }
}

module.exports = {
    NotFoundError,
    ValidationError,
    UnauthorizedError
};
