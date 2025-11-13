import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { RouterModule, Router } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MeetingService } from '../../services/meeting.service';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-meeting-list',
  templateUrl: './meeting-list.component.html',
  styleUrls: ['./meeting-list.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatTooltipModule,
    MatDialogModule,
    MatSnackBarModule
  ]
})
export class MeetingListComponent implements OnInit {
  displayedColumns: string[] = [
    'meetingId',
    'topic',
    'startTime',
    'duration',
    'timezone',
    'agenda',
    'hostEmail',
    'createdAt',
    'actions'
  ];
  //  displayedColumns: string[] = [
  //   'id',
  //   'meetingId',
  //   'topic',
  //   'type',
  //   'startTime',
  //   'duration',
  //   'timezone',
  //   'agenda',
  //   'joinUrl',
  //   'startUrl',
  //   'password',
  //   'hostEmail',
  //   'createdAt',
  //   'updatedAt',
  //   'isDeleted',
  //   'hostVideo',
  //   'participantVideo',
  //   'joinBeforeHost',
  //   'muteUponEntry',
  //   'waitingRoom',
  //   'approvalType',
  //   'autoRecording',
  //   'actions'
  // ];
  dataSource!: MatTableDataSource<any>;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private router: Router,
    private meetingService: MeetingService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadMeetings();
  }

  loadMeetings(): void {
    this.meetingService.getMeetings().subscribe(
      (meetings) => {
        this.dataSource = new MatTableDataSource(meetings);
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
      },
      (error) => {
        this.snackBar.open('Error loading meetings', 'Close', { duration: 3000 });
      }
    );
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  viewMeeting(id: number) {
    this.router.navigate(['/meetings', id]);
  }

  editMeeting(id: number) {
    this.router.navigate(['/meetings', id, 'edit']);
  }

  deleteMeeting(id: number) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '300px',
      data: {
        title: 'Confirm Delete',
        message: 'Are you sure you want to delete this meeting?'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.meetingService.deleteMeeting(id).subscribe(
          () => {
            this.loadMeetings();
            this.snackBar.open('Meeting deleted successfully', 'Close', { duration: 3000 });
          },
          (error) => {
            this.snackBar.open('Error deleting meeting', 'Close', { duration: 3000 });
          }
        );
      }
    });
  }

  createMeeting() {
    this.router.navigate(['/meetings/new']);
  }
}