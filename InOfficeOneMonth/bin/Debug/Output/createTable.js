
var rndColor = function () {
    var color = 'rgb(' + (Math.floor((256 - 199) * Math.random()) + 200) + ',' + (Math.floor((256 - 199) * Math.random()) + 200) + ',' + (Math.floor((256 - 199) * Math.random()) + 200) + ')';

    return color;
};
var createTable = function (week) {
    for (var i = 0; i < 5; i++) {
        $('a:eq(' + i + ')').on('click', function (event) {
            if (event.target.id < 4) {
                displayWeek(event.target.id);
            }
            else {
                DysplayTotal();
            };
        });
    };
    $('#cont table').remove();
    $('#description').html('Employment Week number ' + (++week));
    --week;
    $('<table>').attr({ 'id': 'baseTable', 'cellpadding': "2", 'cellspacing': '3', 'border': '1' })
    .css('width', '100%').appendTo('#cont');
    $('<tr>').appendTo('#baseTable');
    $('<th rowspan="2">№</th>').appendTo('#baseTable tr:first');
    $('<th rowspan="2">Employee Name</th>').appendTo('#baseTable tr:first');
    for (var i = 1; i <= 5; i++) {
        $('<th colspan="8">Day ' + i + '</th>').appendTo('#baseTable tr:first');
    }
    $('<th rowspan="2">Week Salary</th>').appendTo('#baseTable tr:first');
    $('<tr>').appendTo('#baseTable');
    for (var i = 0; i < 5; i++) {
        for (var j = 1; j <= 8; j++) {
            $('<th> H' + j + '</th>').appendTo('#baseTable tr:eq(1)');
        }
    }

    var data = jsonData.Employment.Weeks[week].Employees;
    $.each(data, function (i, object) {
        var trCurrent = document.createElement('tr');
        var ordTD = document.createElement('td');
        ordTD.innerHTML = i + 1;
        $(trCurrent).append(ordTD);
        var nameTD = document.createElement('td');
        nameTD.innerHTML = object.Name;
        $(trCurrent).append(nameTD);
        $.each(object.Responsibility, function (j, value) {

            for (var k = 0; k < 8; k++) {
                var empTD = document.createElement('td');
                empTD.innerHTML = value[k];
                $(trCurrent).append(empTD);
            }
        });
        var tdSalary = document.createElement('td');
        tdSalary.innerHTML = object.Weeksalary;
        $(trCurrent).append(tdSalary);
        $('#baseTable').append(trCurrent);
    });

    var tdToltips = $("#baseTable tr:gt(1) td:not(:nth-child(43), :nth-child(2), :nth-child(1))");

    $.each($(tdToltips), function (i, object) {
        $(object).hover(function (event) {
            // Hover over code
            var titleText = respArray[$(this).text()];

            $('<p class="tooltip"></p>')
      .text(titleText)
      .appendTo('body')
      .css('top', (event.pageY - 10) + 'px')
      .css('left', (event.pageX + 20) + 'px')
      .fadeIn('slow');
        }, function () {
            $('.tooltip').remove();
        }).mousemove(function (event) {
            // Mouse move code
            $('.tooltip')
      .css('top', (event.pageY - 10) + 'px')
      .css('left', (event.pageX + 20) + 'px');
        });
    });
};

var displayWeek = function (week) {
    var curdata = jsonData.Employment.Weeks[week].Employees;

    $.each(curdata, function (i, object) {
        var elementTR = 'tr:eq(' + (i + 2) + ') ';
        $.each(object.Responsibility, function (j, value) {

            for (var k = 0; k < 8; k++) {
                var elementTD = 'td:eq(' + (j * 8 + k + 2) + ') ';
                var find = elementTR + elementTD;
                $(find).html(value[k]);

            }
            $(elementTR + 'td:eq(42)').html(object.Weeksalary);
        });
    });
    $('#description').html('Employment Week number ' + (++week)).css('background',rndColor());
};

var DysplayTotal = function () {

    $('#description').html('Total Salary one Month').css('background',rndColor());
    for (var i = 0; i < 4; i++) {
        $('a:eq(' + i + ')').on('click', function (event) {
            createTable(event.target.id);
            $('#descr_2').fadeIn('slow');
        });
    }
    $('#cont table').remove();
    $('<table>').attr({ 'id': 'totalTable', 'cellpadding': "2", 'cellspacing': '3', 'border': '1' })
    .css( 'width', '50%').appendTo('#cont');
    $('<tr>').appendTo('#totalTable');
    $('<th> № </th>').appendTo('#totalTable tr:first');
    $('<th>Employee Name</th>').appendTo('#totalTable tr:first');
    for (var i = 1; i <= 4; i++) {
        $('<th>Week ' + i + '</th>').appendTo('#totalTable tr:first');
    }
    $('<th>Month Salary </th>').appendTo('#totalTable tr:first');
    var monthSalary = [];
    for (var j = 0; j < 4; j++) {
        monthSalary[j] = [];
        $.each(jsonData.Employment.Weeks[j].Employees, function (n, object) {
            monthSalary[j][n] = object.Weeksalary;
        });
    };
    var jarray = jsonData.Employment.Weeks[0].Employees;
    $.each(jarray, function (i, object) {
        var trCurrent = document.createElement('tr');
        var wsTD = document.createElement('td');
        wsTD.innerHTML = i + 1;
        $(trCurrent).append(wsTD);
        var nameTD = document.createElement('td');
        nameTD.innerHTML = object.Name;
        $(trCurrent).append(nameTD);
        var total = 0;
        $.each(monthSalary, function (m, value) {
            $('<td>').html(value[i]).appendTo(trCurrent);
            total += value[i];
        });
        $('<td>').html(total).appendTo(trCurrent);
        $('#totalTable').append(trCurrent);
    });
    $('#totalTable').css({ 'position': 'absolute', 'left': ($(window).width() - $('#totalTable').outerWidth()) / 2 });
    $('#descr_2').fadeOut();
};