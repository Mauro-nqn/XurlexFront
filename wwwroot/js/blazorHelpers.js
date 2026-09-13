window.blazorHelpers = {
    getWidth: function () {
        return window.innerWidth;
    }
};


window.blazorHelpers.esMovilReal = () => {
    return (
        window.matchMedia("(pointer: coarse)").matches ||
        /iphone|ipad|ipod|android/i.test(navigator.userAgent)
    );
};





