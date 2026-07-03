# user-shell-component.html

<mat-sidenav-container>
  <!-- SIDEBAR -->
  <mat-sidenav
    #sidenav
    [mode]="isMobile() ? 'over' : 'side'"
    [opened]="isMobile() ? sidebarOpen() : true"
    aria-label="Customer navigation">
    <mat-toolbar color="primary">Hotel</mat-toolbar>
    <mat-nav-list>
      <a mat-list-item routerLink="/user/dashboard" routerLinkActive="active" (click)="onNavClick()">
        <mat-icon matListItemIcon aria-hidden="true">dashboard</mat-icon>
        <span matListItemTitle>Dashboard</span>
      </a>
      <a mat-list-item routerLink="/user/bookings" routerLinkActive="active" (click)="onNavClick()">
        <mat-icon matListItemIcon aria-hidden="true">book_online</mat-icon>
        <span matListItemTitle>My Bookings</span>
      </a>
      <a mat-list-item routerLink="/user/room-service" routerLinkActive="active" (click)="onNavClick()">
        <mat-icon matListItemIcon aria-hidden="true">room_service</mat-icon>
        <span matListItemTitle>Room Service</span>
      </a>
    </mat-nav-list>
  </mat-sidenav>

  <!-- MAIN CONTENT -->
  <mat-sidenav-content>
    <mat-toolbar color="primary">
      @if (isMobile()) {
        <button mat-icon-button (click)="sidebarOpen.set(!sidebarOpen())">
          <mat-icon aria-hidden="true">menu</mat-icon>
        </button>
      }
      <span>Hotel</span>
      <span class="spacer"></span>
      <button mat-icon-button [matMenuTriggerFor]="userMenu" aria-label="Open user menu">
        <mat-icon aria-hidden="true">account_circle</mat-icon>
      </button>
      <mat-menu #userMenu="matMenu">
        <button mat-menu-item routerLink="/user/profile">
          <mat-icon aria-hidden="true">manage_accounts</mat-icon> Profile
        </button>
        <button mat-menu-item (click)="logout()">
          <mat-icon aria-hidden="true">logout</mat-icon> Logout
        </button>
      </mat-menu>
    </mat-toolbar>

    <!-- ROUTER OUTLET -->
    <div class="content">
      <router-outlet></router-outlet>

      <footer class="site-footer">
        <div class="footer-links">
          <a href="#">Privacy Policy</a>
          <a href="#">Terms of Service</a>
          <a href="#">Press</a>
          <a href="#">Careers</a>
          <a href="#">Contact</a>
        </div>
        <div class="footer-logo">AETHERIS</div>
        <div class="footer-info">
          <span>1 AETHERIS PEAK, THE SILENT VALLEY</span>
          <span class="separator"></span>
          <span>&copy; 2024 AETHERIS. ALL RIGHTS RESERVED.</span>
        </div>
      </footer>
    </div>

  </mat-sidenav-content>
</mat-sidenav-container>

# user-shell-component.scss

@import '../../../styles/theme/index';

mat-sidenav-container {
height: 100vh;
width: 100%;
}

mat-sidenav {
width: 250px;
border-right: 1px solid rgba(0, 0, 0, 0.12);

mat-toolbar {
position: sticky;
top: 0;
z-index: 2;
}
}

mat-sidenav-content {
display: flex;
flex-direction: column;
height: 100%;

mat-toolbar {
position: sticky;
top: 0;
z-index: 2;
}
}

.spacer {
flex: 1 1 auto;
}

.content {
padding: 24px;
flex-grow: 1;
overflow-y: auto;
box-sizing: border-box;
}

.active {
background-color: rgba(63, 81, 181, 0.08);
color: #3f51b5 !important;
font-weight: 500;

mat-icon {
color: #3f51b5;
}
}

@media (max-width: 1024px) {
.content {
padding: 16px;
}
}

.site-footer {
background: var(--color-surface-container-lowest);
padding: 6rem 1rem 3rem;
text-align: center;
border-top: 1px solid var(--glass-border);
margin-top: 4rem; // Add spacing above the footer inside content container

.footer-links {
display: flex;
flex-wrap: wrap;
justify-content: center;
gap: 2rem;
margin-bottom: 3rem;
a {
@include font-body-md;
color: var(--color-on-tertiary-container);
text-decoration: none;
transition: color 0.3s;
&:hover { color: var(--color-secondary); }
}
@media (max-width: 768px) {
gap: 1.2rem;
a { font-size: 0.85rem; }
}
}

.footer-logo {
font-family: var(--font-headline);
font-size: clamp(3rem, 10vw, 7.5rem);
letter-spacing: 0.3em;
color: var(--color-on-surface);
margin-bottom: 1.5rem;
text-transform: uppercase;
}

.footer-info {
font-family: var(--font-body);
font-size: 0.625rem;
font-weight: 500;
letter-spacing: 0.3em;
text-transform: uppercase;
color: rgba(228, 226, 221, 0.4);
display: flex;
flex-wrap: wrap;
justify-content: center;
align-items: center;
gap: 1.5rem;
.separator {
display: inline-block;
width: 4px;
height: 4px;
border-radius: 50%;
background: rgba(228, 226, 221, 0.2);
}
}
}

# dashboard-component.html

<div class="dashboard">
  <!-- Welcome message -->
  @if (loading()) {
    <mat-spinner diameter="40"></mat-spinner>
  } @else if (error()) {
    <app-alert type="error" [message]="error()!" (closed)="error.set(null)">
      <button mat-button (click)="loadDashboard()">Retry</button>
    </app-alert>
  } @else {
    <h1>Welcome back, Mr {{ firstName() }}</h1>

    <div class="booking-cards">
      <!-- Current Booking (CheckedIn) -->
      @if (currentBooking()) {
        <mat-card class="booking-card current">
          <mat-card-header>
            <mat-card-title>Current Stay</mat-card-title>
            <mat-card-subtitle>Room: {{ getRoomNumbers(currentBooking()!) }}</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <p><strong>Check&#8209;in:</strong> {{ currentBooking()!.checkInDate }}</p>
            <p><strong>Check&#8209;out:</strong> {{ currentBooking()!.checkOutDate }}</p>
            <p><strong>Status:</strong> {{ currentBooking()!.bookingStatus }}</p>
          </mat-card-content>
          <mat-card-actions>
            <button mat-raised-button color="accent" (click)="openServiceRequest('housekeeping')" aria-label="Request housekeeping">
              <mat-icon>cleaning_services</mat-icon> Request Housekeeping
            </button>
            <button mat-raised-button color="warn" (click)="openServiceRequest('maintenance')" aria-label="Request maintenance">
              <mat-icon>build</mat-icon> Request Maintenance
            </button>
          </mat-card-actions>
        </mat-card>
      } @else {
        <mat-card class="booking-card no-booking">
          <mat-card-content>
            <p>No active stay right now.</p>
          </mat-card-content>
        </mat-card>
      }

      <!-- Upcoming Booking (Booked) -->
      @if (upcomingBooking()) {
        <mat-card class="booking-card upcoming">
          <mat-card-header>
            <mat-card-title>Upcoming Stay</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p><strong>Check&#8209;in:</strong> {{ upcomingBooking()!.checkInDate }}</p>
            <p><strong>Check&#8209;out:</strong> {{ upcomingBooking()!.checkOutDate }}</p>
            <p><strong>Status:</strong> {{ upcomingBooking()!.bookingStatus }}</p>
            @if (upcomingRoomTypes().length > 0) {
              <p><strong>Room Type(s):</strong> {{ upcomingRoomTypes().join(', ') }}</p>
            }
          </mat-card-content>
        </mat-card>
      } @else {
        <mat-card class="booking-card no-booking">
          <mat-card-content>
            <p>No upcoming bookings.</p>
          </mat-card-content>
        </mat-card>
      }
    </div>

    @if (currentBooking()) {
      <div class="room-service-status">
        <h2>Room Service Status</h2>
        <div class="status-grid">
          <!-- Housekeeping -->
          <mat-card class="status-card">
            <mat-card-header>
              <mat-card-title>Housekeeping</mat-card-title>
              <mat-card-subtitle>{{ pendingHousekeeping().length }} pending / in-progress</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              @for (item of pendingHousekeeping(); track item.id) {
                <div class="status-item">
                  <p class="status-line">
                    <span>{{ item.description || 'No description' }}</span>
                    <span class="badge" [class]="item.status.toLowerCase()">{{ item.status }}</span>
                  </p>
                </div>
              } @empty {
                <p class="no-status-items">No pending requests.</p>
              }
            </mat-card-content>
          </mat-card>

          <!-- Maintenance -->
          <mat-card class="status-card">
            <mat-card-header>
              <mat-card-title>Maintenance</mat-card-title>
              <mat-card-subtitle>{{ pendingMaintenance().length }} pending / in-progress</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              @for (item of pendingMaintenance(); track item.id) {
                <div class="status-item">
                  <p class="status-line">
                    <span>{{ item.description || 'No description' }}</span>
                    <span class="badge" [class]="item.status.toLowerCase()">{{ item.status }}</span>
                  </p>
                </div>
              } @empty {
                <p class="no-status-items">No pending requests.</p>
              }
            </mat-card-content>
          </mat-card>

          <!-- Food Orders -->
          <mat-card class="status-card">
            <mat-card-header>
              <mat-card-title>Food Orders</mat-card-title>
              <mat-card-subtitle>{{ pendingFoodOrders().length }} preparing / pending</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              @for (order of pendingFoodOrders(); track order.id) {
                <div class="status-item">
                  <p class="status-line">
                    <span>Order #{{ order.id }}</span>
                    <span class="badge" [class]="(order.orderStatus || 'pending').toLowerCase()">{{ order.orderStatus || 'Pending' }}</span>
                  </p>
                </div>
              } @empty {
                <p class="no-status-items">No pending orders.</p>
              }
            </mat-card-content>
          </mat-card>
        </div>
      </div>
    }

}

</div>

# dashboard-component.scss

.dashboard {
padding: 24px;

h1 {
margin-bottom: 24px;
font-size: 1.75rem;
font-weight: 500;
}

.booking-cards {
display: flex;
flex-wrap: wrap;
gap: 16px;

    .booking-card {
      flex: 1 1 300px;

      mat-card-actions {
        display: flex;
        flex-wrap: wrap;
        gap: 8px;
        padding: 8px 16px 16px;
      }
    }

}

.room-service-status {
margin-top: 32px;

    h2 {
      font-size: 1.4rem;
      font-weight: 500;
      margin-bottom: 16px;
    }

    .status-grid {
      display: flex;
      flex-wrap: wrap;
      gap: 16px;

      .status-card {
        flex: 1 1 300px;
        max-width: 100%;
        box-shadow: 0 2px 4px rgba(0,0,0,0.05);

        mat-card-header {
          margin-bottom: 12px;
          border-bottom: 1px solid #f0f0f0;
          padding-bottom: 8px;
        }

        .status-item {
          padding: 8px 0;
          border-bottom: 1px dashed #f0f0f0;
          &:last-child {
            border-bottom: none;
          }

          p {
            margin: 0;
          }

          .status-line {
            display: flex;
            justify-content: space-between;
            align-items: center;
            font-size: 0.95rem;
          }

          .description {
            font-size: 0.85rem;
            color: rgba(0, 0, 0, 0.54);
            margin-top: 4px;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
          }

          .badge {
            display: inline-block;
            padding: 2px 6px;
            border-radius: 4px;
            font-size: 0.8rem;
            font-weight: 500;

            &.pending {
              background-color: #fff3e0;
              color: #e65100;
            }
            &.inprogress, &.preparing {
              background-color: #e8eaf6;
              color: #1a237e;
            }
            &.completed {
              background-color: #e8f5e9;
              color: #1b5e20;
            }
          }
        }

        .no-status-items {
          text-align: center;
          color: rgba(0, 0, 0, 0.54);
          font-style: italic;
          margin: 16px 0 8px;
        }
      }
    }

}
}

@media (max-width: 599px) {
.dashboard {
.room-service-status {
.status-grid {
flex-direction: column;
}
}
}
}

