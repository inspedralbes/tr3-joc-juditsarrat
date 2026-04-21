const IUserStatsRepository = require('./IUserStatsRepository');

/**
 * Implementació en memòria per a estadístiques.
 */
class UserStatsRepositoryInMemory extends IUserStatsRepository {
    constructor() {
        super();
        this.stats = [];
    }

    async findByUserId(userId) {
        for (let i = 0; i < this.stats.length; i++) {
            if (this.stats[i].userId === userId) {
                return this.stats[i];
            }
        }
        return null;
    }

    async updateStats(userId, statsData) {
        for (let i = 0; i < this.stats.length; i++) {
            if (this.stats[i].userId === userId) {
                // Actualitzem camps
                if (statsData.totalGames !== undefined) this.stats[i].totalGames = statsData.totalGames;
                if (statsData.wins !== undefined) this.stats[i].wins = statsData.wins;
                if (statsData.losses !== undefined) this.stats[i].losses = statsData.losses;
                if (statsData.totalScore !== undefined) this.stats[i].totalScore = statsData.totalScore;
                if (statsData.avgScore !== undefined) this.stats[i].avgScore = statsData.avgScore;
                this.stats[i].updatedAt = new Date();
                return this.stats[i];
            }
        }
        // Si no existeix, el creem (upsert)
        return await this.create({ userId, ...statsData });
    }

    async create(statsData) {
        const newEntry = {
            ...statsData,
            totalGames: statsData.totalGames || 0,
            wins: statsData.wins || 0,
            losses: statsData.losses || 0,
            totalScore: statsData.totalScore || 0,
            avgScore: statsData.avgScore || 0,
            updatedAt: new Date()
        };
        this.stats.push(newEntry);
        return newEntry;
    }

    async getTopPlayers(limit) {
        // Ordenació manual per wins descendent
        const sorted = [...this.stats].sort((a, b) => {
            if (b.wins !== a.wins) return b.wins - a.wins;
            return b.totalScore - a.totalScore;
        });
        return sorted.slice(0, limit || 10);
    }
}

module.exports = UserStatsRepositoryInMemory;
