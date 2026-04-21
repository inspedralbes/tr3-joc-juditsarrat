const mongoose = require('mongoose');

//estadistiques agregades d'un usuari
const userStatsSchema = new mongoose.Schema({
    userId: {
        type: String,
        required: true,
        unique: true
    },
    totalGames: {
        type: Number,
        default: 0
    },
    wins: {
        type: Number,
        default: 0
    },
    losses: {
        type: Number,
        default: 0
    },
    totalScore: {
        type: Number,
        default: 0
    },
    avgScore: {
        type: Number,
        default: 0
    },
    updatedAt: {
        type: Date,
        default: Date.now
    }
});

// Middleware per actualitzar la data de modificació automàticament
userStatsSchema.pre('save', function () {
    this.updatedAt = new Date();
});

module.exports = mongoose.model('UserStats', userStatsSchema);
