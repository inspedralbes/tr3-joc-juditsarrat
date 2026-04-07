const mongoose = require('mongoose');

//emmagatzemar el resultat final d'una partifa
const resultSchema = new mongoose.Schema({
    gameId: {
        type: String,
        required: true,
        unique: true
    },
    winnerId: {
        type: String,
        required: true
    },
    players: [
        {
            playerId: { type: String, required: true },
            score: { type: Number, default: 0 },
            position: { type: Number }
        }
    ],
    duration: {
        type: Number, // Segons que ha durat la partida
        default: 0
    },
    createdAt: {
        type: Date,
        default: Date.now
    }
});

module.exports = mongoose.model('Result', resultSchema);
