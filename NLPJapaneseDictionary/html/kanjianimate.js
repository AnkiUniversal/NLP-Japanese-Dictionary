var svgDiv = null;
var vivusElement = null;
var svgElement = null;
var totalDuration = 0;

function startAnimation(speed) {
    if (vivusElement.getStatus() == 'end')
        vivusElement.reset();

    vivusElement.play(parseFloat(speed));
}

function stopAnimation() {
    vivusElement.stop();
}

function resetAnimation(speed) {
    vivusElement.stop().reset().play(parseFloat(speed));
}

function changeSVG(svg, strokeCount) {

    if (vivusElement != null)
        vivusElement.destroy();

    if (svgDiv == null)
        svgDiv = document.getElementById('kanjisvg');

    svgDiv.innerHTML = svg;
    svgElement = svgDiv.getElementsByTagName("svg")[0];
    svgElement.id = 'svg';
    totalDuration = parseInt(strokeCount) * 30;
    vivusElement = new Vivus(svgElement.id, { type: 'oneByOne', duration: totalDuration, start: 'manual' });
}

function changeReadMode(readMode) {
    if (readMode == 'day')
        document.body.className = 'day';
    else
        document.body.className = 'night';
}