window.printDiv = (divId, title) => {
    const elem = document.getElementById(divId);
    if (!elem) return;

    const contents = elem.innerHTML;
    const win = window.open('', '', 'height=800,width=1200');

    const html = `
        <html>
        <head>
            <title>${title || 'Informe'}</title>
        </head>
        <body>
            ${contents}
        </body>
        </html>
    `;

    win.document.open();
    win.document.write(html);
    win.document.close();
    win.focus();
    win.print();
};
