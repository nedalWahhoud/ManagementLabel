// download Etikett
window.exportEtikett = async function (elementId, fileName, format) {
            const element = document.getElementById(elementId);
            if (!element) return;

            const canvas = await html2canvas(element);
            const dataURL = canvas.toDataURL("image/png");
    // get element width and height in px
    const widthPx = element.offsetWidth;
    const heightPx = element.offsetHeight;
    // get element width and height in mm
    const mmPerPx = 0.264583;
    const pdfWidth = widthPx * mmPerPx;
    const pdfHeight = heightPx * mmPerPx;
    //  pdfHeight = pdfHeight + (pdfWidth - pdfHeight);
            if (format === "png") {
                const link = document.createElement('a');
                link.href = dataURL;
                link.download = `${fileName}.png`;
                link.click();
            } else if (format === "pdf") {
                const { jsPDF } = window.jspdf;
                // Landscape: The width is expected to be longer than the length.
                const pdf = new jsPDF({
                    orientation: 'Landscape', 
                    unit: 'mm',
                    format: [pdfWidth, pdfHeight]
                });
                // first convert to png
                pdf.addImage(dataURL, 'PNG', 0, 0, pdfWidth, pdfHeight);
                // than to pdf
                pdf.save(`${fileName}.pdf`);
            }
        };

// download barcode
window.downloadBase64Image = async function (base64Data, fileName, type) {
    const byteCharacters = atob(base64Data);
    const byteArrays = [];

    for (let offset = 0; offset < byteCharacters.length; offset += 1024) {
        const slice = byteCharacters.slice(offset, offset + 1024);
        const byteNumbers = new Array(slice.length);

        for (let i = 0; i < slice.length; i++) {
            byteNumbers[i] = slice.charCodeAt(i);
        }

        byteArrays.push(new Uint8Array(byteNumbers));
    }

    let mimeType = '';
    if (type === 'pdf') {
        mimeType = 'application/pdf';
    } else {
        mimeType = 'image/png';
    }

    const blob = new Blob(byteArrays, { type: mimeType });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = `${fileName}.${type}`;
    link.click();
};
