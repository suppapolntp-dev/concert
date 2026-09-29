import { Directive, Input, TemplateRef, ViewContainerRef, inject } from '@angular/core';
import { AuthService } from '../../core/auth/auth.service';

@Directive({
  selector: '[hasPermission]',
  standalone: true
})
export class HasPermissionDirective {
  private readonly templateRef = inject(TemplateRef<unknown>);
  private readonly viewContainer = inject(ViewContainerRef);
  private readonly authService = inject(AuthService);
  private isVisible = false;

  @Input() set hasPermission(permission: string) {
    const hasPerm = this.authService.hasPermission(permission);
    if (hasPerm && !this.isVisible) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.isVisible = true;
    } else if (!hasPerm && this.isVisible) {
      this.viewContainer.clear();
      this.isVisible = false;
    }
  }
}
