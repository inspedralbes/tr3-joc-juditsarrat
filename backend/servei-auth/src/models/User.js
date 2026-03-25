const mongoose = require('mongoose');
const bcrypt = require('bcryptjs');

const userSchema = new mongoose.Schema({
    username: {
        type: String,
        required: true,
        unique: true
    },
    email: {
        type: String,
        required: true,
        unique: true
    },
    passwordHash: {
        type: String,
        required: true
    },
    createdAt: {
        type: Date,
        default: Date.now
    },

});


userSchema.pre('save', async function (next) {
    const user = this;

    if (!user.isModified('passwordHash')) return next();

    try {

        const salt = await bcrypt.genSalt(10);
        const hash = await bcrypt.hash(user.passwordHash, salt);

      
        user.passwordHash = hash;
        next();
    } catch (err) {
        next(err);
    }
});


userSchema.methods.comparePassword = async function (candidatePassword) {
    try {
        return await bcrypt.compare(candidatePassword, this.passwordHash);
    } catch (err) {
        throw new Error(err);
    }
};

// Exportem el model
const User = mongoose.model('User', userSchema);
module.exports = User;
