import { Injectable, signal } from '@angular/core';

export interface SeatStatusUpdate {
  seatId: string;
  status: 'Available' | 'Held' | 'Reserved' | 'Sold';
  heldByUserId?: string;
  holdExpiresAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  readonly isConnected = signal<boolean>(false);
  readonly latestSeatUpdate = signal<SeatStatusUpdate | null>(null);

  startConnection(hubUrl: string): void {
    console.log(`[SignalRService] Connecting to ${hubUrl}... (skeleton)`);
    this.isConnected.set(true);
  }

  stopConnection(): void {
    console.log('[SignalRService] Disconnected.');
    this.isConnected.set(false);
  }

  joinEventRoom(eventId: string): void {
    console.log(`[SignalRService] Joined room for event: ${eventId}`);
  }

  leaveEventRoom(eventId: string): void {
    console.log(`[SignalRService] Left room for event: ${eventId}`);
  }
}
