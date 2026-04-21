const jwt = require('jsonwebtoken');


const JWT_SECRET = process.env.JWT_SECRET || 'super-secret-key-123';

//genera token per usuari
function signToken(payload, expiresIn) {
    return jwt.sign(payload, JWT_SECRET, {
        expiresIn: expiresIn || '24h'
    });
}

//verificar token
function verifyToken(token) {
    try {
        return jwt.verify(token, JWT_SECRET);
    } catch (err) {
        return null;
    }
}

module.exports = {
    signToken,
    verifyToken
};
