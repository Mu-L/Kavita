import { ChangeDetectionStrategy, Component, OnInit, OnDestroy, HostListener, Output, EventEmitter } from '@angular/core';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { ImageService } from 'src/app/_services/image.service';
import { BehaviorSubject } from 'rxjs';

@Component({
  selector: 'app-cover-masonry',
  templateUrl: './cover-masonry.component.html',
  styleUrls: ['./cover-masonry.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [CommonModule, NgOptimizedImage]
})
export class CoverMasonryComponent implements OnInit, OnDestroy {
  @Output() hasValidImages = new EventEmitter<boolean>();
  coverImages: string[] = [];
  duplicateImages: string[] = [];
  noImagesFound = false;
  private readonly batchSize = 40;
  private loading = false;
  private scrollContainer: HTMLElement | null = null;
  private loadedImages = 0;

  constructor(public imageService: ImageService) {}

  ngOnInit() {
    this.loadRandomCovers();
    this.scrollContainer = document.querySelector('.masonry-container');
  }

  ngOnDestroy() {
    this.scrollContainer = null;
  }

  @HostListener('window:scroll', ['$event'])
  onScroll() {
    if (this.loading || !this.scrollContainer) return;
    
    const container = this.scrollContainer;
    const scrollPosition = window.scrollY + window.innerHeight;
    const containerBottom = container.offsetTop + container.offsetHeight;
    
    // Load more when we're within 200px of the bottom
    if (scrollPosition > containerBottom - 200) {
      this.loadRandomCovers();
    }
  }

  private loadRandomCovers() {
    if (this.loading) return;
    
    this.loading = true;
    this.loadedImages = 0;
    const newImages: string[] = [];
    
    for (let i = 0; i < this.batchSize; i++) {
      const url = this.imageService.getRandomSeriesCoverImage();
      if (url) {
        // Add a random query parameter to prevent caching
        const uniqueUrl = `${url}?t=${Date.now()}`;
        newImages.push(uniqueUrl);
      }
    }

    // Check if images load successfully
    newImages.forEach(url => {
      const img = new Image();
      img.onload = () => {
        this.loadedImages++;
        if (this.loadedImages === newImages.length) {
          this.coverImages = [...this.coverImages, ...newImages];
          this.duplicateImages = [...this.coverImages];
          this.noImagesFound = this.coverImages.length === 0;
          this.hasValidImages.emit(true);
        }
      };
      img.onerror = () => {
        this.loadedImages++;
        if (this.loadedImages === newImages.length) {
          this.noImagesFound = this.coverImages.length === 0;
          this.hasValidImages.emit(this.coverImages.length > 0);
        }
      };
      img.src = url;
    });

    this.loading = false;
  }
} 