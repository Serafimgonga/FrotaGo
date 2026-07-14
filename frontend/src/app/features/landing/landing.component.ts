import { Component, OnInit, OnDestroy, signal, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-landing',
  imports: [CommonModule, RouterModule],
  templateUrl: './landing.html',
  styleUrl: './landing.css'
})
export class LandingComponent implements OnInit, OnDestroy {
  scrolled = signal(false);
  menuOpen = signal(false);
  activeSection = signal('hero');
  countersStarted = false;

  stats = [
    { value: 0, target: 98, suffix: '%', label: 'Taxa de Satisfação', icon: 'sentiment_very_satisfied' },
    { value: 0, target: 5000, suffix: '+', label: 'Veículos Geridos', icon: 'directions_car' },
    { value: 0, target: 300, suffix: '+', label: 'Escolas de Condução', icon: 'school' },
    { value: 0, target: 60, suffix: '%', label: 'Redução de Custos', icon: 'trending_down' },
  ];

  features = [
    {
      icon: 'directions_car',
      title: 'Gestão de Frota',
      description: 'Monitorize todos os seus veículos em tempo real. Estados, quilometragens e histórico completo de cada viatura da sua frota.',
      color: '#6366f1'
    },
    {
      icon: 'people',
      title: 'Instrutores & Alunos',
      description: 'Cadastro completo de instrutores e alunos, com categorias de carta, histórico de aulas e progresso individual.',
      color: '#8b5cf6'
    },
    {
      icon: 'event',
      title: 'Agendamento de Aulas',
      description: 'Agende aulas práticas com facilidade. Verifique a disponibilidade de veículos e instrutores antes de confirmar.',
      color: '#a78bfa'
    },
    {
      icon: 'build',
      title: 'Manutenção',
      description: 'Registe manutenções preventivas e corretivas. Acompanhe custos e histórico técnico de cada veículo.',
      color: '#22d3ee'
    },
    {
      icon: 'local_gas_station',
      title: 'Controlo de Combustível',
      description: 'Registe abastecimentos, analise o consumo médio e controle os custos de combustível por veículo.',
      color: '#34d399'
    },
    {
      icon: 'description',
      title: 'Conformidade Legal',
      description: 'Alertas automáticos para seguros, inspecções e DUA a expirar. Esteja sempre em conformidade com a lei angolana.',
      color: '#f59e0b'
    }
  ];

  testimonials = [
    {
      name: 'António Mbemba',
      role: 'Director — AutoEscola Luanda Norte',
      text: 'O FrotaGo transformou completamente a nossa gestão. Antes perdíamos horas em papelada, agora temos tudo digitalizado e acessível em segundos.',
      avatar: 'A'
    },
    {
      name: 'Conceição Neto',
      role: 'Gestora — Escola de Condução Viana',
      text: 'Os alertas de expiração de documentos salvaram-nos de multas. O sistema avisa-nos com 30 dias de antecedência. Indispensável!',
      avatar: 'C'
    },
    {
      name: 'Pedro Domingos',
      role: 'Proprietário — Auto Escola Benguela',
      text: 'Reduzimos os custos de combustível em 40% só com o módulo de controlo. O investimento retornou no primeiro mês de uso.',
      avatar: 'P'
    }
  ];

  workflow = [
    { step: '01', title: 'Cadastre a sua frota', description: 'Registe todos os veículos, instrutores e alunos numa plataforma centralizada.', icon: 'app_registration' },
    { step: '02', title: 'Agende e monitorize', description: 'Agende aulas práticas, manutenções e gerencie a disponibilidade em tempo real.', icon: 'schedule' },
    { step: '03', title: 'Analise e optimize', description: 'Receba relatórios detalhados de custos, desempenho e conformidade legal.', icon: 'analytics' },
  ];

  private animationFrames: number[] = [];

  constructor(private router: Router) {}

  ngOnInit(): void {}

  ngOnDestroy(): void {
    this.animationFrames.forEach(id => cancelAnimationFrame(id));
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
      const duration = 2000;
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

  toggleMenu(): void {
    this.menuOpen.update(v => !v);
  }

  navigateToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }

  navigateToLogin(): void {
    this.router.navigate(['/login']);
  }

  scrollTo(sectionId: string): void {
    const el = document.getElementById(sectionId);
    if (el) el.scrollIntoView({ behavior: 'smooth' });
    this.menuOpen.set(false);
  }
}
