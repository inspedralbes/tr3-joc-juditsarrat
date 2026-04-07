const mongoose = require('mongoose');

/**
 * Esquema per a una partida de joc.
 */
const gameSchema = new mongoose.Schema({
    hostId: {
        type: String,
        required: true
    },
    status: {
        type: String,
        enum: ['waiting', 'playing', 'finished'],
        default: 'waiting'
    },
    players: {
        type: [String],
        default: []
    },
    config: {
        type: Object,
        default: {
            maxPlayers: 2,
            mapType: 'default'
        }
    }
}, {
    timestamps: true
});

const Game = mongoose.model('Game', gameSchema);
module.exports = Game;
