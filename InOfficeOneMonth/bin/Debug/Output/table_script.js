$(document).ready(function () {

    $('<div>').addClass('linkContainer').prependTo('#cont');
    $('<p>)', { id: 'description' }).css(
        { 'display': 'inline-block', 'fontSize': '13pt',
            'width': '250px', 'color': 'rgb(137, 45, 210)'
        })
        .prependTo('.linkContainer');

    window.respArray = ["None", "WriteCode", "DrawLayout", "TestProgram", "SellServices", "CreateReports", "GivenTask"];

    for (var i = 0; i < 5; i++) {
        var newdiv = document.createElement('div');
        $(newdiv).addClass('weekLink').appendTo('.linkContainer');
        $('<a>', {
            id: i,
            text: (i != 4) ? 'Week_' + (i + 1) + '         ' : 'Total one Month',
            href: '#'
        }).appendTo(newdiv);
        $(newdiv).appendTo('.linkContainer');
    };

    $('<p>)', { id: 'descr_2' }).css(
        { 'display': 'inline-block', 'fontSize': '13pt',
            'color': 'rgb(208, 27, 96)'
        })
        .css({ 'left': ($(window).width() - $('#descr_2').outerWidth()) / 2 })
        .appendTo('#cont');
    var str = "Responsibility: ";
    for (var i = 0; i < respArray.length; i++) {
        str += ' ' + i + ' : ' + respArray[i] + ',';
    };
    $('#descr_2').text(str.replace(new RegExp(",$"), ""));

    createTable(0);

    $(".linkContainer > div,p").each(function () { $(this).css('background', rndColor()) });

});
