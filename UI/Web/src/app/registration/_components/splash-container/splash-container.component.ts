import {ChangeDetectionStrategy, Component, inject, OnInit} from '@angular/core';
import {AsyncPipe, NgStyle} from "@angular/common";
import {NavService} from "../../../_services/nav.service";
import {CoverMasonryComponent} from "../cover-masonry/cover-masonry.component";
import {ThemeService} from "../../../_services/theme.service";

@Component({
    selector: 'app-splash-container',
    templateUrl: './splash-container.component.html',
    styleUrls: ['./splash-container.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [
        NgStyle,
        AsyncPipe,
        CoverMasonryComponent
    ]
})
export class SplashContainerComponent implements OnInit {
  protected readonly navService = inject(NavService);
  protected readonly themeService = inject(ThemeService);
  hasValidCoverImages = false;

  ngOnInit() {
    this.themeService.getThemes().subscribe();
  }

  onCoverImagesValid(hasImages: boolean) {
    this.hasValidCoverImages = !hasImages;
  }
}
