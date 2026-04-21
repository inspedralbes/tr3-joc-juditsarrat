const IUserRepository = require('./IUserRepository');

class UserRepositoryInMemory extends IUserRepository {
    constructor() {
        super();
        this.users = [];
    }

    async findById(id) {
        for (let i = 0; i < this.users.length; i++) {
            const user = this.users[i];
            if (user._id === id || user.id === id) {
                return user;
            }
        }
        return null;
    }

    async findByEmail(email) {
        for (let i = 0; i < this.users.length; i++) {
            const user = this.users[i];
            if (user.email === email) {
                return user;
            }
        }
        return null;
    }

    async findByUsername(username) {
        for (let i = 0; i < this.users.length; i++) {
            const user = this.users[i];
            if (user.username === username) {
                return user;
            }
        }
        return null;
    }

    async create(userData) {
        const id = "user_" + Date.now() + "_" + Math.floor(Math.random() * 1000);
        const newUser = {
            username: userData.username,
            email: userData.email,
            passwordHash: userData.passwordHash,
            _id: userData._id || id,
            createdAt: new Date()
        };
        this.users.push(newUser);
        return newUser;
    }

    async update(id, userData) {
        for (let i = 0; i < this.users.length; i++) {
            const user = this.users[i];
            if (user._id === id || user.id === id) {
                if (userData.username) user.username = userData.username;
                if (userData.email) user.email = userData.email;
                if (userData.passwordHash) user.passwordHash = userData.passwordHash;
                return user;
            }
        }
        return null;
    }

    async delete(id) {
        for (let i = 0; i < this.users.length; i++) {
            const user = this.users[i];
            if (user._id === id || user.id === id) {
                this.users.splice(i, 1);
                return true;
            }
        }
        return false;
    }
}

module.exports = UserRepositoryInMemory;
