const mongoose = require('mongoose');

const schema = new mongoose.Schema({ name: String });
schema.pre('save', function(next) {
    console.log('in pre save');
    try {
        next();
    } catch (e) {
        console.error('error calling next:', e.message);
    }
});
const Model = mongoose.model('Test', schema);

async function run() {
    await mongoose.connect('mongodb://localhost:27017/test_db', {});
    const doc = new Model({ name: 'test' });
    try {
        await doc.save();
        console.log('saved ok');
    } catch (err) {
        console.error('save error:', err);
    }
    await mongoose.disconnect();
}
run();
