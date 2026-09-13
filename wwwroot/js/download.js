window.downloadFromBytes = (base64, contentType, fileName) => {
    const link = document.createElement("a");
    link.href = `data:${contentType};base64,${base64}`;
    link.download = fileName;
    link.click();
    link.remove();
};

