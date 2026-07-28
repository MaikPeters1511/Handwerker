import { ChangeDetectionStrategy, Component, computed, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import {OfferService} from './services/offer.service';
import {Offer, OfferStatus} from "../../core/entities";

type FilterType = 'all' | 'sent' | 'received';

@Component({
  selector: 'app-offers',
  imports: [CommonModule],
  templateUrl: './offers.html',
  styleUrl: './offers.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Offers implements OnInit {
  offerService = inject(OfferService);
  router = inject(Router);

  offers = signal<Offer[]>([]);
  filter = signal<FilterType>('all');
  loading = signal(true);

  filteredOffers = computed(() => {
    const filterValue = this.filter();
    const allOffers = this.offers();

    switch (filterValue) {
      case 'sent':
        return allOffers.filter(o => !o.isReceived);
      case 'received':
        return allOffers.filter(o => o.isReceived);
      default:
        return allOffers;
    }
  });

  ngOnInit() {
    this.loadOffers();
  }

  loadOffers() {
    this.loading.set(true);
    this.offerService.getOffers().subscribe({
      next: (offers) => {
        this.offers.set(offers);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Fehler beim Laden der Angebote:', err);
        this.loading.set(false);
      }
    });
  }

  onFilterChange(filter: FilterType) {
    this.filter.set(filter);
  }

  onCreate() {
    this.router.navigate(['/offers/new']);
  }

  onEdit(id: number) {
    this.router.navigate(['/offers', id]);
  }

  onDelete(id: number) {
    if (confirm('Möchten Sie dieses Angebot wirklich löschen?')) {
      this.offerService.deleteOffer(id).subscribe({
        next: () => {
          this.loadOffers(); // Refresh list
        },
        error: (err) => {
          console.error('Fehler beim Löschen:', err);
          alert('Fehler beim Löschen des Angebots');
        }
      });
    }
  }

  onConvertToOrder(id: number) {
    if (confirm('Möchten Sie dieses Angebot in einen Auftrag umwandeln?')) {
      this.offerService.convertToOrder(id).subscribe({
        next: (result) => {
          alert(result.message);
          this.loadOffers(); // Refresh list
        },
        error: (err) => {
          console.error('Fehler bei Umwandlung:', err);
          alert('Fehler bei der Umwandlung in einen Auftrag');
        }
      });
    }
  }

  getStatusBadgeClass(status: OfferStatus): string {
    const baseClasses = 'badge';
    switch (status) {
      case 'Draft':
        return `${baseClasses} badge-ghost`;
      case 'Sent':
        return `${baseClasses} badge-info`;
      case 'Accepted':
        return `${baseClasses} badge-success`;
      case 'Declined':
        return `${baseClasses} badge-error`;
      case 'Converted':
        return `${baseClasses} badge-primary`;
      default:
        return baseClasses;
    }
  }

  getStatusLabel(status: OfferStatus): string {
    const labels: Record<OfferStatus, string> = {
      'Draft': 'Entwurf',
      'Sent': 'Versendet',
      'Accepted': 'Angenommen',
      'Declined': 'Abgelehnt',
      'Converted': 'Umgewandelt'
    };
    return labels[status] || status;
  }

  formatDate(date: Date | string): string {
    if (!date) return '';
    const d = typeof date === 'string' ? new Date(date) : date;
    return d.toLocaleDateString('de-DE');
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('de-DE', {
      style: 'currency',
      currency: 'EUR'
    }).format(amount);
  }
}
