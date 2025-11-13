import { Routes } from '@angular/router';
import { MeetingListComponent } from './components/meeting-list/meeting-list.component';
import { MeetingDetailsComponent } from './components/meeting-details/meeting-details.component';
import { MeetingFormComponent } from './components/meeting-form/meeting-form.component';

export const routes: Routes = [
  { path: 'meetings', component: MeetingListComponent },
  { path: 'meetings/new', component: MeetingFormComponent },
  { path: 'meetings/:id', component: MeetingDetailsComponent },
  { path: 'meetings/:id/edit', component: MeetingFormComponent },
  { path: '', redirectTo: '/meetings', pathMatch: 'full' }
];
