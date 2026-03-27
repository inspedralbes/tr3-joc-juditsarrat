const IUserRepository = require('./IUserRepository');
const User = require('../models/User');


class UserRepository extends IUserRepository {

    async findById(id) {
        return await User.findById(id);
    }


    async findByEmail(email) {
        return await User.findOne({ email });
    }


    async findByUsername(username) {
        return await User.findOne({ username });
    }


    async create(userData) {
        const user = new User(userData);
        return await user.save();
    }


    async update(id, userData) {
        return await User.findByIdAndUpdate(id, userData, { new: true });
    }


    async delete(id) {
        const result = await User.findByIdAndDelete(id);
        return result !== null;
    }
}

module.exports = UserRepository;
