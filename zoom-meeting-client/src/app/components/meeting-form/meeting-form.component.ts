import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MeetingService } from '../../services/meeting.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';

@Component({
  selector: 'app-meeting-form',
  templateUrl: './meeting-form.component.html',
  styleUrls: ['./meeting-form.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatDatepickerModule,
    MatNativeDateModule
  ]
})
export class MeetingFormComponent implements OnInit {
  meetingForm!: FormGroup;
  isEditMode = false;
  meetingId?: number;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private meetingService: MeetingService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.meetingId = Number(this.route.snapshot.paramMap.get('id'));
    
    if (this.meetingId) {
      this.isEditMode = true;
      this.loadMeeting();
    }
  }

  private initializeForm(): void {
    this.meetingForm = this.fb.group({
      topic: ['', [Validators.required]],
      startTime: ['', [Validators.required]],
      startTimeTime: ['', [Validators.required]],
      duration: ['', [Validators.required, Validators.min(15)]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      description: ['']
    });
  }

  private combineDateAndTime(date: Date, timeString: string): Date {
    const [hours, minutes] = timeString.split(':').map(Number);
    const combined = new Date(date);
    combined.setHours(hours, minutes);
    return combined;
  }

  loadMeeting(): void {
    this.isLoading = true;
    this.meetingService.getMeeting(this.meetingId!).subscribe(
      (meeting) => {
        const startTime = new Date(meeting.startTime);
        const timeString = startTime.getHours().toString().padStart(2, '0') + ':' + 
                          startTime.getMinutes().toString().padStart(2, '0');
        
        this.meetingForm.patchValue({
          ...meeting,
          startTimeTime: timeString
        });
        this.isLoading = false;
      },
      (error) => {
        this.snackBar.open('Error loading meeting', 'Close', { duration: 3000 });
        this.isLoading = false;
      }
    );
  }

  onSubmit(): void {
    if (this.meetingForm.valid) {
      const formValue = this.meetingForm.value;
      const combinedDateTime = this.combineDateAndTime(formValue.startTime, formValue.startTimeTime);
      
      const meetingData = {
        ...formValue,
        startTime: combinedDateTime,
      };
      delete meetingData.startTimeTime;
      
      this.isLoading = true;
      
      if (this.isEditMode) {
        this.meetingService.updateMeeting(this.meetingId!, meetingData).subscribe(
          (response) => {
            this.snackBar.open('Meeting updated successfully', 'Close', { duration: 3000 });
            this.router.navigate(['/meetings', this.meetingId]);
            this.isLoading = false;
          },
          (error) => {
            this.snackBar.open('Error updating meeting', 'Close', { duration: 3000 });
            this.isLoading = false;
          }
        );
      } else {
        this.meetingService.createMeeting(meetingData).subscribe(
          (response) => {
            this.snackBar.open('Meeting created successfully', 'Close', { duration: 3000 });
            this.router.navigate(['/meetings']);
            this.isLoading = false;
          },
          (error) => {
            this.snackBar.open('Error creating meeting', 'Close', { duration: 3000 });
            this.isLoading = false;
          }
        );
      }
    }
  }

  onCancel(): void {
    if (this.isEditMode) {
      this.router.navigate(['/meetings', this.meetingId]);
    } else {
      this.router.navigate(['/meetings']);
    }
  }
}