//Validacions reutilitzables

function isValidEmail(email) {
    if (!email) return false;
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return regex.test(email);
}

function isValidUsername(username) {
    if (!username) return false;

    return username.length >= 3;
}

function isValidPassword(password) {
    if (!password) return false;
    return password.length >= 6;
}

module.exports = {
    isValidEmail,
    isValidUsername,
    isValidPassword
};
