window.printEtikett = function (elementId) {
    const element = document.getElementById(elementId);
    if (!element) return;

    const css = `
.etikett {
    width: 600px;
    padding: 15px;
    border: 1px solid #000;
    border-radius: 8px;
    background-color: #fff;
    font-family: Arial, sans-serif;
    box-shadow: 0 0 8px rgba(0,0,0,0.1);
    text-align: center;
    margin: 20px auto;
}
    }
    .etikett img {
        max-width: 100%;
        height: auto;
        margin-bottom: 10px;
    }
productInfo.{
    width:auto;
    height:auto;
}
.productInfo p {
    margin: 4px 0;
    font-weight:bold;
    font-size: 18px;
}
`;

    const printWindow = window.open('', '_blank');
    printWindow.document.write(`
        <html>
            <head>
                <title></title>
                <style>
                    ${css}
                </style>
            </head>
            <body>
                ${element.outerHTML}
            </body>
        </html>
    `);
    printWindow.document.close();

    printWindow.focus();
    printWindow.print();
    printWindow.close();
};

window.printBarcode = function (elementId) {
    const element = document.getElementById(elementId);
    if (!element) return;

    const css = `
       body {
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin: 0;
        }

        .imgBarcode {
            width: 600px;
            height: 200px;
            display: block;
        }
`;

    const printWindow = window.open('', '_blank');
    printWindow.document.write(`
        <html>
            <head>
                <title></title>
                <style>
                    ${css}
                </style>
            </head>
            <body>
                ${element.outerHTML}
            </body>
        </html>
    `);
    printWindow.document.close();

    printWindow.focus();
    printWindow.print();
    printWindow.close();
};
