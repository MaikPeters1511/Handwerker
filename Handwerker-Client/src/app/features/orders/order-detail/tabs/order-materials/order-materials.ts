import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import type { Order, OrderMaterial } from '../../../../../core/entities/order.model';
import type { Article, Warehouse } from '../../../../../core/entities/article.model';
import { ArticleService, OrderService } from '../../../../../core/services';

@Component({
  selector: 'app-order-materials',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './order-materials.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OrderMaterials {
  private fb = inject(FormBuilder);
  private orderService = inject(OrderService);
  private articleService = inject(ArticleService);

  order = input<Order | null>(null);

  materials = computed(() => this.order()?.materials || []);
  articles = signal<Article[]>([]);
  warehouses = signal<Warehouse[]>([]);
  loading = signal(false);
  showAddForm = signal(false);

  form: FormGroup = this.fb.group({
    articleId: ['', Validators.required],
    warehouseId: ['', Validators.required],
    plannedQuantity: [0, [Validators.required, Validators.min(0.01)]]
  });

  constructor() {
    this.loadArticles();
    this.loadWarehouses();
  }

  loadArticles() {
    this.articleService.getActiveArticles().subscribe({
      next: (articles: Article[]) => this.articles.set(articles),
      error: (err: unknown) => console.error('Error loading articles:', err)
    });
  }

  loadWarehouses() {
    this.articleService.getActiveWarehouses().subscribe({
      next: (warehouses: Warehouse[]) => this.warehouses.set(warehouses),
      error: (err: unknown) => console.error('Error loading warehouses:', err)
    });
  }

  onAddMaterial() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const orderId = this.order()?.id;
    if (!orderId) return;

    this.loading.set(true);
    this.orderService.addMaterial(orderId, {
      articleId: this.form.value.articleId,
      warehouseId: this.form.value.warehouseId,
      plannedQuantity: this.form.value.plannedQuantity
    }).subscribe({
      next: () => {
        this.loading.set(false);
        this.showAddForm.set(false);
        this.form.reset({ plannedQuantity: 0 });
        // Reload order to get updated materials
        window.location.reload(); // Quick fix - in production use proper state management
      },
      error: (err: unknown) => {
        this.loading.set(false);
        const errorMessage = err && typeof err === 'object' && 'error' in err ?
          (err as { error?: { error?: string } }).error?.error : 'Unbekannter Fehler';
        alert('Fehler beim Hinzufügen: ' + errorMessage);
      }
    });
  }

  onConfirmMaterial(materialId: number, actualQty: number) {
    this.orderService.confirmMaterial(materialId, { actualQuantity: actualQty }).subscribe({
      next: () => window.location.reload(),
      error: (err: unknown) => {
        const errorMessage = err && typeof err === 'object' && 'error' in err ?
          (err as { error?: { error?: string } }).error?.error : 'Unbekannter Fehler';
        alert('Fehler: ' + errorMessage);
      }
    });
  }

  getArticleName(articleId: number): string {
    return this.articles().find(a => a.id === articleId)?.name || 'Unbekannt';
  }

  getArticleNumber(articleId: number): string {
    return this.articles().find(a => a.id === articleId)?.articleNumber || '';
  }

  getWarehouseName(warehouseId: number): string {
    return this.warehouses().find(w => w.id === warehouseId)?.name || 'Unbekannt';
  }

  getStockInfo(articleId: number, warehouseId: number): string {
    // In production, fetch actual stock
    return 'Verfügbar';
  }
}
