am4core.ready(function () {

    $.ajax({
        type: "POST",
        url: "/ChartReports/AppStageGrouping",
        contentType: false,
        processData: false,
        async: false,
        success: function (response) {
            // Themes begin
            //am4core.useTheme(am4themes_moonrisekingdom);
            am4core.useTheme(am4themes_animated);
            // Themes end

            // Create chart
            var chart = am4core.create("chartdiv", am4charts.PieChart);
            chart.hiddenState.properties.opacity = 0; // this creates initial fade-in

            chart.data = response;

            chart.legend = new am4charts.Legend();
            chart.exporting.menu = new am4core.ExportMenu();

            var series = chart.series.push(new am4charts.PieSeries());
            series.dataFields.value = "appCount";
            series.dataFields.radiusValue = "appCount";
            series.dataFields.category = "stageName";
            series.slices.template.cornerRadius = 6;
            series.colors.step = 3;

            series.hiddenState.properties.endAngle = -90;

            chart.legend.maxWidth = 10;
        },
        error: function (response) {
          
        },
        complete: function () {
           
        }

    });

}); // end am4core.ready()



am4core.ready(function () {

    $.ajax({
        type: "POST",
        url: "/ChartReports/AppStageGrouping",
        contentType: false,
        processData: false,
        async: false,
        success: function (response) {
            // Themes begin
            am4core.useTheme(am4themes_dataviz);
            am4core.useTheme(am4themes_animated);
            // Themes end

            // Create chart
            var chart = am4core.create("chartdiv3D", am4charts.PieChart3D);
            chart.hiddenState.properties.opacity = 0; // this creates initial fade-in
            chart.exporting.menu = new am4core.ExportMenu();

            chart.data = response;
            chart.legend = new am4charts.Legend();
            chart.innerRadius = 70;

            var series = chart.series.push(new am4charts.PieSeries3D());
            series.dataFields.value = "appCount";
            series.dataFields.category = "stageName";
        },
        error: function (response) {

        },
        complete: function () {

        }

    });

}); 





am4core.ready(function () {

    var datas = [];

    var text = "";

    $.ajax({
        type: "POST",
        url: "/ChartReports/AppStatus",
        contentType: false,
        processData: false,
        async: false,
        success: function (response) {

            am4core.useTheme(am4themes_material);
            am4core.useTheme(am4themes_animated);
            // Themes end

            var chart = am4core.create("StatusDiv", am4plugins_forceDirected.ForceDirectedTree);
            var networkSeries = chart.series.push(new am4plugins_forceDirected.ForceDirectedSeries())

            chart.data = response;

            chart.legend = new am4charts.Legend();
            chart.exporting.menu = new am4core.ExportMenu();

            networkSeries.dataFields.value = "value";
            networkSeries.dataFields.name = "name";
            networkSeries.dataFields.children = "children";
            networkSeries.nodes.template.tooltipText = "{name}:{value}";
            networkSeries.nodes.template.fillOpacity = 1;
            networkSeries.dataFields.id = "name";
            networkSeries.dataFields.linkWith = "linkWith";

            networkSeries.nodes.template.label.text = "{name}";
            networkSeries.fontSize = 15;

            var selectedNode;

            var label = chart.createChild(am4core.Label);
            label.text = "Click on nodes to link";
            label.x = 50;
            label.y = 50;
            label.isMeasured = false;

            networkSeries.nodes.template.events.on("up", function (event) {
                var node = event.target;
                if (!selectedNode) {
                    node.outerCircle.disabled = false;
                    node.outerCircle.strokeDasharray = "3,3";
                    selectedNode = node;
                }
                else if (selectedNode === node) {
                    node.outerCircle.disabled = true;
                    node.outerCircle.strokeDasharray = "";
                    selectedNode = undefined;
                }
                else {
                    var node2 = event.target;

                    var link = node2.linksWith.getKey(selectedNode.uid);

                    if (link) {
                        node2.unlinkWith(selectedNode);
                    }
                    else {
                        node2.linkWith(selectedNode, 0.2);
                    }
                }
            });

        },
        error: function (response) {

        },
        complete: function () {

        }

    });

});





am4core.ready(function () {

    var datas = [];

    var text = "";

    $.ajax({
        type: "POST",
        url: "/ChartReports/ApplicationStagesByMonth",
        contentType: false,
        processData: false,
        async: false,
        success: function (response) {

            am4core.useTheme(am4themes_animated);

            // Themes end

            // Create chart instance
            var chart = am4core.create("AppMonthStage", am4charts.XYChart);
            chart.scrollbarX = new am4core.Scrollbar();
            chart.exporting.menu = new am4core.ExportMenu();

            // Increase contrast by taking evey second color
            chart.colors.step = 2;

            // Add data
            chart.data = response;

            // Create axes
            var dateAxis = chart.xAxes.push(new am4charts.DateAxis());
            dateAxis.renderer.minGridDistance = 50;

            // Create series
            function createAxisAndSeries(field, name, opposite, bullet) {
                var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
                if (chart.yAxes.indexOf(valueAxis) !== 0) {
                    valueAxis.syncWithAxis = chart.yAxes.getIndex(0);
                }

                var series = chart.series.push(new am4charts.LineSeries());
                series.dataFields.valueY = field;
                series.dataFields.dateX = 'date';
                series.strokeWidth = 2;
                series.yAxis = valueAxis;
                series.name = name;
                series.tooltipText = "{name}: [bold]{valueY}[/]";
                series.tensionX = 0.8;
                series.showOnInit = true;

                var interfaceColors = new am4core.InterfaceColorSet();

                switch (bullet) {
                    case "triangle":
                        var bullet = series.bullets.push(new am4charts.Bullet());
                        bullet.width = 12;
                        bullet.height = 12;
                        bullet.horizontalCenter = "middle";
                        bullet.verticalCenter = "middle";

                        var triangle = bullet.createChild(am4core.Triangle);
                        triangle.stroke = interfaceColors.getFor("background");
                        triangle.strokeWidth = 2;
                        triangle.direction = "top";
                        triangle.width = 12;
                        triangle.height = 12;
                        break;
                    case "rectangle":
                        var bullet = series.bullets.push(new am4charts.Bullet());
                        bullet.width = 10;
                        bullet.height = 10;
                        bullet.horizontalCenter = "middle";
                        bullet.verticalCenter = "middle";

                        var rectangle = bullet.createChild(am4core.Rectangle);
                        rectangle.stroke = interfaceColors.getFor("background");
                        rectangle.strokeWidth = 2;
                        rectangle.width = 10;
                        rectangle.height = 10;
                        break;
                    default:
                        var bullet = series.bullets.push(new am4charts.CircleBullet());
                        bullet.circle.stroke = interfaceColors.getFor("background");
                        bullet.circle.strokeWidth = 2;
                        break;
                }

                valueAxis.renderer.line.strokeOpacity = 1;
                valueAxis.renderer.line.strokeWidth = 2;
                valueAxis.renderer.line.stroke = series.stroke;
                valueAxis.renderer.labels.template.fill = series.stroke;
                valueAxis.renderer.opposite = opposite;
            }

            
            createAxisAndSeries("new", "New", false, "circle");
            createAxisAndSeries("renewal", "Renewal", true, "triangle");
            createAxisAndSeries("suplementry", "Suplementry", true, "rectangle");

            // Add legend
            chart.legend = new am4charts.Legend();

            // Add cursor
            chart.cursor = new am4charts.XYCursor();

        },
        error: function (response) {

        },
        complete: function () {

        }

    });

});




am4core.ready(function () {

    var datas = [];

    var text = "";

    $.ajax({
        type: "POST",
        url: "/ChartReports/ApplicationsByDays",
        contentType: false,
        processData: false,
        async: false,
        success: function (response) {

            // Themes begin
            am4core.useTheme(am4themes_material);
            am4core.useTheme(am4themes_animated);
            // Themes end

            var chart = am4core.create("AppsByDay", am4charts.XYChart);
            chart.exporting.menu = new am4core.ExportMenu();
            chart.paddingRight = 20;

            chart.data = response;

            var dateAxis = chart.xAxes.push(new am4charts.DateAxis());
            dateAxis.renderer.grid.template.location = 0;
            dateAxis.renderer.axisFills.template.disabled = true;
            dateAxis.renderer.ticks.template.disabled = true;

            var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
            valueAxis.tooltip.disabled = true;
            valueAxis.renderer.minWidth = 35;
            valueAxis.renderer.axisFills.template.disabled = true;
            valueAxis.renderer.ticks.template.disabled = true;

            var series = chart.series.push(new am4charts.LineSeries());
            series.dataFields.dateX = "date";
            series.dataFields.valueY = "appCount";
            series.strokeWidth = 2;
            series.tooltipText = "value: {valueY}, day change: {valueY.previousChange}";

            // set stroke property field
            series.propertyFields.stroke = "color";

            chart.cursor = new am4charts.XYCursor();

            chart.scrollbarX = new am4core.Scrollbar();
            dateAxis.keepSelection = true;


        },
        error: function (response) {

        },
        complete: function () {

        }

    });

});




am4core.ready(function () {

    $.ajax({
        type: "POST",
        url: "/ChartReports/AppStatusByYear",
        contentType: false,
        processData: false,
        async: false,
        success: function (response) {

            // Themes begin
            am4core.useTheme(am4themes_animated);
            // Themes end

            // Create chart instance
            var chart = am4core.create("AppCountByYear", am4charts.XYChart);

            // Add percent sign to all numbers
            chart.numberFormatter.numberFormat = "#";
            chart.exporting.menu = new am4core.ExportMenu();

            // Add data
            chart.data = response;

            // Create axes
            var categoryAxis = chart.xAxes.push(new am4charts.CategoryAxis());
            categoryAxis.dataFields.category = "year";
            categoryAxis.renderer.grid.template.location = 0;
            categoryAxis.renderer.minGridDistance = 30;

            var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
            valueAxis.title.text = "Application Count";
            valueAxis.title.fontWeight = 800;

            // Create series
            var series = chart.series.push(new am4charts.ColumnSeries());
            series.dataFields.valueY = "approved";
            series.dataFields.categoryX = "year";
            series.clustered = true;
            series.name = "Approved";
            series.columns.template.width = am4core.percent(50);
            series.tooltipText = "No of {name} application in {categoryX}: [bold]{valueY}[/]";

            var series2 = chart.series.push(new am4charts.ColumnSeries());
            series2.dataFields.valueY = "processing";
            series2.dataFields.categoryX = "year";
            series2.clustered = true;
            series2.name = "Processing";
            series2.columns.template.width = am4core.percent(50);
            series2.tooltipText = "No of {name} application in {categoryX}: [bold]{valueY}[/]";

            var series3 = chart.series.push(new am4charts.ColumnSeries());
            series3.dataFields.valueY = "rejected";
            series3.dataFields.categoryX = "year";
            series3.name = "Rejected";
            series3.clustered = true;
            series3.columns.template.width = am4core.percent(50);
            series3.tooltipText = "No of {name} application in {categoryX}: [bold]{valueY}[/]";

            var series4 = chart.series.push(new am4charts.ColumnSeries());
            series4.dataFields.valueY = "paymentPending";
            series4.dataFields.categoryX = "year";
            series4.clustered = true;
            series4.name = "Payment Pending";
            series4.columns.template.width = am4core.percent(50);
            series4.tooltipText = "No of {name} application in {categoryX}: [bold]{valueY}[/]";

            var series5 = chart.series.push(new am4charts.ColumnSeries());
            series5.dataFields.valueY = "documentsRequired";
            series5.dataFields.categoryX = "year";
            series5.clustered = true;
            series5.name = "Documents Required";
            series5.columns.template.width = am4core.percent(50);
            series5.tooltipText = "No of {name} application in {categoryX}: [bold]{valueY}[/]";

            var series6 = chart.series.push(new am4charts.ColumnSeries());
            series6.dataFields.valueY = "truckRequired";
            series6.dataFields.categoryX = "year";
            series6.clustered = true;
            series6.name = "Trucks Required";
            series6.columns.template.width = am4core.percent(50);
            series6.tooltipText = "No of {name} application in {categoryX}: [bold]{valueY}[/]";

            var series7 = chart.series.push(new am4charts.ColumnSeries());
            series7.dataFields.valueY = "bargeRequired";
            series7.dataFields.categoryX = "year";
            series7.clustered = true;
            series7.name = "Barges Required";
            series7.columns.template.width = am4core.percent(50);
            series7.tooltipText = "No of {name} application in {categoryX}: [bold]{valueY}[/]";

            chart.cursor = new am4charts.XYCursor();
            chart.cursor.lineX.disabled = false;
            chart.cursor.lineY.disabled = false;

            chart.scrollbarX = new am4core.Scrollbar();
        },
        error: function (response) {

        },
        complete: function () {

        }

    });

});




am4core.ready(function () {

    $.ajax({
        type: "POST",
        url: "/ChartReports/TotalMeterForTripOperationsByMonth",
        contentType: false,
        processData: false,
        async: false,
        success: function (response) {

            // Themes begin
            am4core.useTheme(am4themes_kelly);
            am4core.useTheme(am4themes_animated);
            // Themes end

            // Create chart instance
            var chart = am4core.create("CrudeOilTripsDiv", am4charts.XYChart3D);
            chart.exporting.menu = new am4core.ExportMenu();

            // Add data
            chart.data = response;

            // Create axes
            var categoryAxis = chart.xAxes.push(new am4charts.CategoryAxis());
            categoryAxis.dataFields.category = "month";
            categoryAxis.renderer.grid.template.location = 0;
            categoryAxis.renderer.grid.template.strokeOpacity = 0;

            var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
            valueAxis.renderer.grid.template.strokeOpacity = 0;
            valueAxis.min = -10;
            valueAxis.max = 110;
            valueAxis.strictMinMax = true;
            valueAxis.renderer.baseGrid.disabled = true;
            valueAxis.renderer.labels.template.adapter.add("text", function (text) {
                if ((text > 100) || (text < 0)) {
                    return "";
                }
                else {
                    return text + "%";
                }
            })

            // Create series
            var series1 = chart.series.push(new am4charts.ConeSeries());
            series1.dataFields.valueY = "value1";
            series1.dataFields.categoryX = "month";
            series1.columns.template.width = am4core.percent(80);
            series1.columns.template.fillOpacity = 0.9;
            series1.columns.template.strokeOpacity = 1;
            series1.columns.template.strokeWidth = 2;

            var series2 = chart.series.push(new am4charts.ConeSeries());
            series2.dataFields.valueY = "value2";
            series2.dataFields.categoryX = "month";
            series2.stacked = true;
            series2.columns.template.width = am4core.percent(80);
            series2.columns.template.fill = am4core.color("#000");
            series2.columns.template.fillOpacity = 0.1;
            series2.columns.template.stroke = am4core.color("#000");
            series2.columns.template.strokeOpacity = 0.2;
            series2.columns.template.strokeWidth = 2;


            chart.scrollbarX = new am4core.Scrollbar();
        },
        error: function (response) {

        },
        complete: function () {

        }

    });

});




am4core.ready(function () {

    $.ajax({
        type: "POST",
        url: "/ChartReports/GetPermitsByYear",
        contentType: false,
        processData: false,
        async: false,
        success: function (response) {

            // Themes begin
            am4core.useTheme(am4themes_animated);
            // Themes end

            // Create chart instance
            var chart = am4core.create("PermitsPerYear", am4charts.XYChart);

            // Add data
            chart.data = response;

            // Populate data
            for (var i = 0; i < (chart.data.length - 1); i++) {
                chart.data[i].valueNext = chart.data[i + 1].value;
            }

            // Create axes
            var categoryAxis = chart.xAxes.push(new am4charts.CategoryAxis());
            categoryAxis.dataFields.category = "year";
            categoryAxis.renderer.grid.template.location = 0;
            categoryAxis.renderer.minGridDistance = 30;

            var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
            valueAxis.min = 0;

            // Create series
            var series = chart.series.push(new am4charts.ColumnSeries());
            series.dataFields.valueY = "value";
            series.dataFields.categoryX = "year";

            // Add series for showing variance arrows
            var series2 = chart.series.push(new am4charts.ColumnSeries());
            series2.dataFields.valueY = "valueNext";
            series2.dataFields.openValueY = "value";
            series2.dataFields.categoryX = "year";
            series2.columns.template.width = 1;
            series2.fill = am4core.color("#555");
            series2.stroke = am4core.color("#555");

            // Add a triangle for arrow tip
            var arrow = series2.bullets.push(new am4core.Triangle);
            arrow.width = 10;
            arrow.height = 10;
            arrow.horizontalCenter = "middle";
            arrow.verticalCenter = "top";
            arrow.dy = -1;

            // Set up a rotation adapter which would rotate the triangle if its a negative change
            arrow.adapter.add("rotation", function (rotation, target) {
                return getVariancePercent(target.dataItem) < 0 ? 180 : rotation;
            });

            // Set up a rotation adapter which adjusts Y position
            arrow.adapter.add("dy", function (dy, target) {
                return getVariancePercent(target.dataItem) < 0 ? 1 : dy;
            });

            // Add a label
            var label = series2.bullets.push(new am4core.Label);
            label.padding(10, 10, 10, 10);
            label.text = "";
            label.fill = am4core.color("#0c0");
            label.strokeWidth = 0;
            label.horizontalCenter = "middle";
            label.verticalCenter = "bottom";
            label.fontWeight = "bolder";

            // Adapter for label text which calculates change in percent
            label.adapter.add("textOutput", function (text, target) {
                var percent = getVariancePercent(target.dataItem);
                return percent ? percent + "%" : text;
            });

            // Adapter which shifts the label if it's below the variance column
            label.adapter.add("verticalCenter", function (center, target) {
                return getVariancePercent(target.dataItem) < 0 ? "top" : center;
            });

            // Adapter which changes color of label to red
            label.adapter.add("fill", function (fill, target) {
                return getVariancePercent(target.dataItem) < 0 ? am4core.color("#c00") : fill;
            });

            function getVariancePercent(dataItem) {
                if (dataItem) {
                    var value = dataItem.valueY;
                    var openValue = dataItem.openValueY;
                    var change = value - openValue;
                    return Math.round(change / openValue * 100);
                }
                return 0;
            }
           

            chart.scrollbarX = new am4core.Scrollbar();
        },
        error: function (response) {

        },
        complete: function () {

        }

    });

});



am4core.ready(function () {

    $.ajax({
        type: "POST",
        url: "/ChartReports/GetPermitsByDay",
        contentType: false,
        processData: false,
        async: false,
        success: function (response) {

            // Themes begin
            am4core.useTheme(am4themes_dataviz);
            am4core.useTheme(am4themes_animated);
            // Themes end

            // Create chart instance
            var chart = am4core.create("PermitsPerDay", am4charts.XYChart);

            // Add data
            chart.data = response;


            // Create axes
            var dateAxis = chart.xAxes.push(new am4charts.DateAxis());
            dateAxis.renderer.minGridDistance = 50;

            var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());

            // Create series
            var series = chart.series.push(new am4charts.LineSeries());
            series.dataFields.valueY = "value";
            series.dataFields.dateX = "date";
            series.strokeWidth = 2;
            series.minBulletDistance = 10;
            series.tooltipText = "{valueY}";
            series.tooltip.pointerOrientation = "vertical";
            series.tooltip.background.cornerRadius = 20;
            series.tooltip.background.fillOpacity = 0.5;
            series.tooltip.label.padding(12, 12, 12, 12);

            // Add scrollbar
            chart.scrollbarX = new am4charts.XYChartScrollbar();
            chart.scrollbarX.series.push(series);

            // Add cursor
            chart.cursor = new am4charts.XYCursor();
            chart.cursor.xAxis = dateAxis;
            chart.cursor.snapToSeries = series;


            chart.scrollbarX = new am4core.Scrollbar();
        },
        error: function (response) {

        },
        complete: function () {

        }

    });

});

   





