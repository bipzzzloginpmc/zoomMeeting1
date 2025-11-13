import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { MeetingService } from '../../services/meeting.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-meeting-details',
  templateUrl: './meeting-details.component.html',
  styleUrls: ['./meeting-details.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ]
})
export class MeetingDetailsComponent implements OnInit {
  meetingId!: number;
  meeting: any;

  constructor(
    private route: ActivatedRoute,
    private meetingService: MeetingService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.meetingId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadMeeting();
  }

  loadMeeting(): void {
    this.meetingService.getMeeting(this.meetingId).subscribe(
      (meeting) => {
        this.meeting = meeting;
      },
      (error) => {
        this.snackBar.open('Error loading meeting details', 'Close', { duration: 3000 });
      }
    );
  }
}