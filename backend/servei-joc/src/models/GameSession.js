const mongoose = require('mongoose');


// Esquema per a l'estat en temps real d'una partida en curs.

const gameSessionSchema = new mongoose.Schema({
    gameId: {
        type: mongoose.Schema.Types.ObjectId,
        ref: 'Game',
        required: true,
        unique: true
    },
    currentTurn: {
        type: Number,
        default: 1
    },
    scores: {
        type: Map,
        of: Number,
        default: {}
    },
    events: {
        type: [Object],
        default: []
    },
    finishedAt: {
        type: Date,
        default: null
    }
});

const GameSession = mongoose.model('GameSession', gameSessionSchema);
module.exports = GameSession;
