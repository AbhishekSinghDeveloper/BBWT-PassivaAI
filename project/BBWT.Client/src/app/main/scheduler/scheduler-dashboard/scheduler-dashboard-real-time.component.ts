import { Component, OnInit, OnDestroy } from "@angular/core";
import * as signalR from "@microsoft/signalr";
import { Chart, registerables } from "chart.js";

@Component({
    selector: "scheduler-dashboard-real-time",
    templateUrl: "./scheduler-dashboard-real-time.component.html",
    styleUrls: ["./scheduler-dashboard-real-time.component.scss"]
})
export class SchedulerDashboardRealTimeComponent implements OnInit, OnDestroy {
    private realtimeChart: Chart | undefined;
    private hubConnection: signalR.HubConnection;

    constructor() {
        Chart.register(...registerables);
    }

    ngOnInit(): void {
        this.initializeRealtimeChart();
        this.startSignalRConnection();
    }

    ngOnDestroy(): void {
        this.hubConnection?.stop();
    }

    private initializeRealtimeChart(): void {
        const chartContext = document.getElementById("realtimeChart") as HTMLCanvasElement;

        this.realtimeChart = new Chart(chartContext, {
            type: "line",
            data: {
                labels: [],
                datasets: [
                    {
                        label: "Succeeded",
                        data: [],
                        borderColor: "rgba(75, 192, 192, 1)",
                        backgroundColor: "rgba(75, 192, 192, 0.2)",
                        fill: true,
                    },
                    {
                        label: "Processing",
                        data: [],
                        borderColor: "rgba(255, 206, 86, 1)",
                        backgroundColor: "rgba(255, 206, 86, 0.2)",
                        fill: true,
                    },
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
                ]
            },
            options: {
                responsive: true,
                scales: {
                    x: {
                        type: "category",
                        title: {
                            display: true,
                            text: "Time"
                        },
                        ticks: {
                            autoSkip: true,
                            maxRotation: 90,
                            minRotation: 0
                        }
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
                },
                plugins: {
                    tooltip: {
                        callbacks: {
                            label: (context) => {
                                const datasetLabel = context.dataset.label || "";
                                const dataPoint = context.raw as { x: string; y: number; };
                                return `${datasetLabel}: ${dataPoint.y} (Job: ${this.jobNames[dataPoint.x] || "Unknown"})`;
                            }
                        }
                    }
                }
            }
        });
    }

    private startSignalRConnection(): void {
        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl("/api/scheduler/jobStatusHub")
            .build();

        this.hubConnection.on("ReceiveJobStatusUpdate", (status: string, jobName: string, executionTime: Date) => {
            const newData = {
                label: jobName,
                value: jobName ? 1 : 0,
                status: status
            };
            this.updateRealtimeChart(newData);
        });

        this.hubConnection.on("ReceiveJobStatusUpdate1", (status: string, jobName: string, executionTime: Date) => {
            const newData = {
                label: jobName,
                value: jobName ? 1 : 0,
                status: status
            };
            this.updateRealtimeChart(newData);
        });

        this.hubConnection.start().catch(err => console.error("Error while starting connection: " + err));
    }

    private jobNames: { [key: string]: string } = {};

    private updateRealtimeChart(newData: { label: string, value: number, status: string }): void {
        const maxPoints = 10;

        const now = new Date().toLocaleTimeString();
        this.realtimeChart?.data.labels?.push(now);

        this.realtimeChart?.data.datasets.forEach((dataset) => {
            if (dataset.label === newData.status) {
                this.jobNames[now] = newData.label;

                (dataset.data as any[]).push({ x: now, y: newData.value });
                dataset.hidden = false;
            }
        });

        if (this.realtimeChart?.data.labels?.length > maxPoints) {
            const removedLabel = this.realtimeChart?.data.labels.shift() as string;
            if (removedLabel) {
                delete this.jobNames[removedLabel];

                this.realtimeChart?.data.datasets.forEach((dataset) => {
                    (dataset.data as any[]).shift();
                });
            }
        }

        this.realtimeChart?.update();
    }
}
