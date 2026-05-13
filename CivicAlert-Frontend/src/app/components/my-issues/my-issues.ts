import { Component, OnInit, inject, signal } from '@angular/core';
import { IssueService } from '../../services/issue';
import { Issue } from '../../models/issue';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-my-issues',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-issues.html',
  styleUrl: './my-issues.scss'
})
export class MyIssuesComponent implements OnInit {
  public issueService = inject(IssueService);
  myIssues = signal<Issue[]>([]);
  isLoading = signal(true);

  ngOnInit() {
    this.issueService.getMyIssues().subscribe({
      next: (data) => {
        this.myIssues.set(data);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }
}