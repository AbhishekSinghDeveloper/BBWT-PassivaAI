import { Component, OnDestroy, OnInit } from "@angular/core";
import { SchedulerService } from "../scheduler.service";
import { Chart, registerables } from "chart.js";
import { DailyData, HourlyData } from "../JobExecutionDetails";

@Component({
    selector: "scheduler-dashboard-history",
    templateUrl: "./scheduler-dashboard-history.component.html",
    styleUrls: ["./scheduler-dashboard-history.component.scss"]
})
export class SchedulerDashboardHistoryComponent implements OnInit, OnDestroy {
    private historyChart: Chart | undefined;
    private view: "day" | "week" = "day";
    private refreshInterval: any;

    constructor(private schedulerJobService: SchedulerService) {
        Chart.register(...registerables);
    }

    ngOnInit(): void {
        this.initializeHistoryChart();
        this.updateHistoryChartData();
        this.setupAutoRefresh();
    }

    ngOnDestroy(): void {
        this.clearAutoRefresh();
    }

    private initializeHistoryChart(): void {
        const chartContext = document.getElementById("historyChart") as HTMLCanvasElement;

        this.historyChart = new Chart(chartContext, {
            type: "line",
            data: {
                labels: [],
                datasets: [
                    {
                        label: "Failed",
                        data: [],
                        borderColor: "rgba(255, 99, 132, 1)",
                        backgroundColor: "rgba(255, 99, 132, 0.2)",
                        fill: true,
                    },
                    {
                        label: "Deleted",
                        data: [],
                        borderColor: "rgba(54, 162, 235, 1)",
                        backgroundColor: "rgba(54, 162, 235, 0.2)",
                        fill: true,
                    },
                    {
                        label: "Succeeded",
                        data: [],
                        borderColor: "rgba(75, 192, 192, 1)",
                        backgroundColor: "rgba(75, 192, 192, 0.2)",
                        fill: true,
                    }
                ]
            },
            options: {
                responsive: true,
                scales: {
                    x: {
                        display: true,
                        title: {
                            display: true,
                            text: this.view === "day" ? "Time" : "Day"
                        },
                        type: "category",
                    },
                    y: {
                        display: true,
                        title: {
                            display: true,
                            text: "Job Count"
                        },
                        min: 0,
                        ticks: {
                            stepSize: 2,
                        }
                    }
                }
            },
        });
    }

    private updateHistoryChartData(): void {
        this.schedulerJobService.getOverview(this.view).then(response => {

            if (this.historyChart) {
                this.historyChart.data.labels = [];
                this.historyChart.data.datasets.forEach((dataset) => dataset.data = []);
            }

            if (this.view === "day") {
                for (let hour = 0; hour < 24; hour++) {
                    this.historyChart?.data.labels?.push(`${hour % 12 === 0 ? 12 : hour % 12}:00 ${hour < 12 ? "AM" : "PM"}`);
                }
                const data = response as HourlyData[];
                this.historyChart?.data.datasets.forEach(dataset => {
                    dataset.data = Array(24).fill(0);
                });

                data.forEach((hourData: HourlyData, index: number) => {
                    this.historyChart.data.datasets[0].data[index] = hourData.Failed || 0;
                    this.historyChart.data.datasets[1].data[index] = hourData.Deleted || 0;
                    this.historyChart.data.datasets[2].data[index] = hourData.Succeeded || 0;
                });

            } else if (this.view === "week") {
                const data = response as DailyData[];

                this.historyChart.data.labels = data.map(d => {
                    const date = new Date(d.date);
                    return date.toLocaleDateString();
                });

                this.historyChart.data.datasets[0].data = data.map(d => d.failed || 0);
                this.historyChart.data.datasets[1].data = data.map(d => d.deleted || 0);
                this.historyChart.data.datasets[2].data = data.map(d => d.succeeded || 0);
            }

            this.historyChart.update();
        }).catch(error => {
        });
    }

    private setupAutoRefresh(): void {
        if (this.view === "day") {
            this.refreshInterval = setInterval(() => {
                this.updateHistoryChartData();
            }, 10000);
        }
    }

    private clearAutoRefresh(): void {
        if (this.refreshInterval) {
            clearInterval(this.refreshInterval);
            this.refreshInterval = null;
        }
    }

    onViewChange(view: "day" | "week"): void {
        this.view = view;
        this.updateHistoryChartData();
        this.clearAutoRefresh();
        this.setupAutoRefresh();
    }
}
