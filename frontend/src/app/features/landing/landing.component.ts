import { Component, OnInit, OnDestroy, AfterViewInit, signal, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-landing',
  imports: [CommonModule, RouterModule],
  templateUrl: './landing.html',
  styleUrl: './landing.css'
})
export class LandingComponent implements OnInit, OnDestroy, AfterViewInit {
  scrolled = signal(false);
  menuOpen = signal(false);
  activeMockupTab = signal<'overview' | 'gps' | 'legal' | 'finance'>('overview');
  pricingPeriod = signal<'monthly' | 'annual'>('monthly');
  openFaqIndex = signal<number | null>(0);
  countersStarted = false;

  private observer?: IntersectionObserver;

  stats = [
    { value: 0, target: 99, suffix: '%', label: 'Conformidade INATRO', icon: 'gavel', color: '#10b981' },
    { value: 0, target: 5000, suffix: '+', label: 'Viaturas Monitorizadas', icon: 'directions_car', color: '#6366f1' },
    { value: 0, target: 350, suffix: '+', label: 'Escolas Parceiras', icon: 'school', color: '#3b82f6' },
    { value: 0, target: 45, suffix: '%', label: 'Redução de Custos', icon: 'trending_down', color: '#f59e0b' },
  ];

  // Escolas de Condução Reais (Cardes Maiores)
  drivingSchools = [
    {
      name: 'AutoEscola Luanda Norte',
      location: 'Luanda — Talatona & Maianga',
      image: 'assets/images/landing/school_center.png',
      vehicles: '28 Viaturas',
      instructors: '12 Instrutores',
      students: '450+ Alunos Ativos',
      legalBadge: '100% INATRO OK',
      highlight: 'Gestão Completa de Frota e Sedes'
    },
    {
      name: 'Escola de Condução Viana',
      location: 'Viana & Zango',
      image: 'assets/images/landing/instructor_lesson.png',
      vehicles: '18 Viaturas',
      instructors: '8 Instrutores',
      students: '280+ Alunos Ativos',
      legalBadge: 'GPS Ao Vivo',
      highlight: 'Monitorização de Aulas Práticas'
    },
    {
      name: 'Auto Escola Benguela Central',
      location: 'Benguela & Lobito',
      image: 'assets/images/landing/fleet_cars.png',
      vehicles: '22 Viaturas',
      instructors: '10 Instrutores',
      students: '320+ Alunos Ativos',
      legalBadge: '-38% Combustível',
      highlight: 'Controlo de Abastecimento e Peças'
    }
  ];

  features = [
    {
      svgPath: 'M8 17H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2v3m4 4v6m0 0-2-2m2 2 2-2M9 12h6M9 16h4',
      title: 'Gestão de Frota',
      description: 'Estado, quilometragem e disponibilidade de viaturas em tempo real.',
    },
    {
      svgPath: 'M9 12.75 11.25 15 15 9.75m-3-7.036A11.959 11.959 0 0 1 3.598 6 11.99 11.99 0 0 0 3 9.749c0 5.592 3.824 10.29 9 11.623 5.176-1.332 9-6.03 9-11.622 0-1.31-.21-2.571-.598-3.751h-.152c-3.196 0-6.1-1.248-8.25-3.285Z',
      title: 'Conformidade INATRO',
      description: 'Alertas automáticos de Seguros, Inspeções e DUA a expirar.',
    },
    {
      svgPath: 'M15 10.5a3 3 0 1 1-6 0 3 3 0 0 1 6 0ZM19.5 10.5c0 7.142-7.5 11.25-7.5 11.25S4.5 17.642 4.5 10.5a7.5 7.5 0 1 1 15 0Z',
      title: 'Rastreamento GPS',
      description: 'Localização e velocidade das viaturas em aulas práticas.',
    },
    {
      svgPath: 'M6.75 3v2.25M17.25 3v2.25M3 18.75V7.5a2.25 2.25 0 0 1 2.25-2.25h13.5A2.25 2.25 0 0 1 21 7.5v11.25m-18 0A2.25 2.25 0 0 0 5.25 21h13.5A2.25 2.25 0 0 0 21 18.75m-18 0v-7.5A2.25 2.25 0 0 1 5.25 9h13.5A2.25 2.25 0 0 1 21 11.25v7.5',
      title: 'Agenda de Aulas',
      description: 'Marcação simples de aulas sem conflitos de horário.',
    },
    {
      svgPath: 'M11.42 15.17 17.25 21A2.652 2.652 0 0 0 21 17.25l-5.877-5.877M11.42 15.17l2.496-3.03c.317-.384.74-.626 1.208-.766M11.42 15.17l-4.655 5.653a2.548 2.548 0 1 1-3.586-3.586l5.654-4.654m5.65-4.649 3.127-3.451a2.25 2.25 0 0 0-3.314-3.027L8.937 8.104',
      title: 'Manutenção',
      description: 'Controlo de revisões, oficinas e custos de peças.',
    },
    {
      svgPath: 'M3.75 13.5V5.25A2.25 2.25 0 0 1 6 3h8.25a2.25 2.25 0 0 1 2.25 2.25V9h.75A2.25 2.25 0 0 1 19.5 11.25v4.5a2.25 2.25 0 0 1-2.25 2.25H4.5a.75.75 0 0 1-.75-.75v-3.75Zm0 0h10.5m-10.5 0V9.75A2.25 2.25 0 0 1 6 7.5h8.25',
      title: 'Combustível',
      description: 'Registo de abastecimentos e métricas de consumo por km.',
    }
  ];

  pricingPlans = [
    {
      id: 'trial',
      name: 'Starter',
      priceMonthly: '0 Kz',
      priceAnnual: '0 Kz',
      period: 'teste 30 dias',
      description: 'Para pequenas escolas digitalizarem a frota.',
      popular: false,
      buttonText: 'Teste Grátis',
      buttonClass: 'btn-secondary',
      features: [
        'Até 5 Veículos',
        'Até 3 Instrutores',
        'Agendamento de Aulas',
        'Alertas de Documentos'
      ]
    },
    {
      id: 'pro',
      name: 'Profissional',
      priceMonthly: '35.000 Kz',
      priceAnnual: '28.000 Kz',
      period: '/ mês',
      description: 'Ideal para escolas em crescimento.',
      popular: true,
      buttonText: 'Escolher Pro',
      buttonClass: 'btn-primary',
      features: [
        'Até 25 Veículos',
        'Instrutores e Alunos Ilimitados',
        'Conformidade Legal Completa',
        'Combustível e Manutenção',
        'Relatórios & WhatsApp'
      ]
    },
    {
      id: 'enterprise',
      name: 'Empresarial',
      priceMonthly: '75.000 Kz',
      priceAnnual: '60.000 Kz',
      period: '/ mês',
      description: 'Para frotas grandes e múltiplas filiais.',
      popular: false,
      buttonText: 'Contactar Vendas',
      buttonClass: 'btn-secondary',
      features: [
        'Veículos Ilimitados',
        'Múltiplas Filiais',
        'Rastreamento GPS ao Vivo',
        'Suporte Dedicado'
      ]
    }
  ];

  faqs = [
    {
      question: 'Cumpre a legislação em Angola?',
      answer: 'Sim, inclui controlo de DUA, seguros e inspeções periódicas do INATRO.'
    },
    {
      question: 'Os dados da escola ficam isolados?',
      answer: 'Sim, cada escola possui isolamento total de dados via Multi-Tenancy.'
    },
    {
      question: 'Funciona no telemóvel?',
      answer: 'Sim, funciona perfeitamente em smartphones, tablets e computadores.'
    }
  ];

  private animationFrames: number[] = [];

  constructor(private router: Router) {}

  ngOnInit(): void {}

  ngAfterViewInit(): void {
    this.setupScrollObserver();
  }

  ngOnDestroy(): void {
    this.animationFrames.forEach(id => cancelAnimationFrame(id));
    if (this.observer) {
      this.observer.disconnect();
    }
  }

  private setupScrollObserver(): void {
    if (typeof window === 'undefined' || !('IntersectionObserver' in window)) return;

    this.observer = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          entry.target.classList.add('revealed');
        }
      });
    }, {
      threshold: 0.12,
      rootMargin: '0px 0px -50px 0px'
    });

    const elementsToReveal = document.querySelectorAll(
      '.reveal-left, .reveal-right, .reveal-bottom, .reveal-scale, .reveal-fade'
    );
    elementsToReveal.forEach(el => this.observer?.observe(el));
  }

  @HostListener('window:scroll')
  onScroll(): void {
    this.scrolled.set(window.scrollY > 40);
    if (!this.countersStarted && window.scrollY > 300) {
      this.countersStarted = true;
      this.animateCounters();
    }
  }

  animateCounters(): void {
    this.stats.forEach((stat, i) => {
      const duration = 1800;
      const start = performance.now();
      const animate = (now: number) => {
        const elapsed = now - start;
        const progress = Math.min(elapsed / duration, 1);
        const eased = 1 - Math.pow(1 - progress, 3);
        this.stats[i].value = Math.round(eased * stat.target);
        if (progress < 1) {
          this.animationFrames.push(requestAnimationFrame(animate));
        }
      };
      this.animationFrames.push(requestAnimationFrame(animate));
    });
  }

  setMockupTab(tab: 'overview' | 'gps' | 'legal' | 'finance'): void {
    this.activeMockupTab.set(tab);
  }

  setPricingPeriod(period: 'monthly' | 'annual'): void {
    this.pricingPeriod.set(period);
  }

  toggleFaq(index: number): void {
    this.openFaqIndex.update(curr => (curr === index ? null : index));
  }

  toggleMenu(): void {
    this.menuOpen.update(v => !v);
  }

  navigateToLogin(): void {
    this.router.navigate(['/login']);
  }

  navigateToRegister(): void {
    this.router.navigate(['/register']);
  }

  scrollTo(sectionId: string): void {
    const el = document.getElementById(sectionId);
    if (el) el.scrollIntoView({ behavior: 'smooth' });
    this.menuOpen.set(false);
  }
}
