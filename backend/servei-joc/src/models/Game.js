const mongoose = require('mongoose');

const gameSchema = new mongoose.Schema({
    hostId: {
        type: String,
        required: true
    },
    gameCode: {
        type: String,
        unique: true
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
        maxPlayers: {
            type: Number,
            default: 2
        },
        mapType: {
            type: String,
            default: 'default'
        }
    }
}, {
    timestamps: true
});

const Game = mongoose.model('Game', gameSchema);

module.exports = Game;