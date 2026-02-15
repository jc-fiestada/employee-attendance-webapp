export type ToastType = "ok" | "warning" | "error";

interface ToastOptions {
    message: string;
    type?: ToastType;
    duration?: number;
}

class ToastManager {
    private container: HTMLDivElement;

    constructor() {
        this.container = document.createElement("div");
        this.container.id = "toast-container";
        document.body.appendChild(this.container);
    }

    show({ message, type = "ok", duration = 3000 }: ToastOptions) {
        const toast = document.createElement("div");
        toast.className = `toast toast-${type}`;
        toast.textContent = message;

        this.container.appendChild(toast);

        requestAnimationFrame(() => {
            toast.classList.add("show");
        });

        setTimeout(() => {
            toast.classList.remove("show");
            toast.classList.add("hide");

            toast.addEventListener("animationend", () => toast.remove());
        }, duration);
    }
}

export const Toast = new ToastManager();
