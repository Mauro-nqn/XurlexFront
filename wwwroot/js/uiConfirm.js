window.ui = window.ui || {};
window.ui.confirm = (msg) => {
    try {
        return window.confirm(msg);
    } catch {
        return false;
    }
};
