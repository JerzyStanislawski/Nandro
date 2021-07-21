﻿using Microsoft.EntityFrameworkCore;
using Nandro.Data;
using Nandro.Models;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Nandro.ViewModels
{
    public class ProductsViewModel : ReactiveObject, IRoutableViewModel
    {
        private readonly NandroDbContext _dbContext;

        public IScreen HostScreen { get; }
        public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);
        public ObservableCollection<string> Units { get; } = new ObservableCollection<string>(Enum.GetNames(typeof(ProductUnit)));
        public CombinedReactiveCommand<Unit, Unit> Save => ReactiveCommand.CreateCombined(new[] { ReactiveCommand.Create(Persist), HostScreen.Router.NavigateBack }, canExecute: Observable.Return(CanSave));
        public ReactiveCommand<Unit, Unit> Add => ReactiveCommand.Create(AddProduct);
        public bool CanSave { get; private set; }

        public ObservableCollection<Product> Products { get; }

        public ProductsViewModel(IScreen hostScreen)
        {
            HostScreen = hostScreen;

            _dbContext = Locator.Current.GetService<NandroDbContext>();
            Products = new ObservableCollection<Product>(_dbContext.Products);
        }

        public void RaisePropertyChanged()
        {
            CanSave = Products.All(x => !String.IsNullOrEmpty(x.Name));
            this.RaisePropertyChanged(nameof(Save));
        }

        private void AddProduct()
        {
            Products.Add(new Product
            {
                Id = Products.Max(x => x.Id) + 1,
                Unit = ProductUnit.Piece
            });
            RaisePropertyChanged();
        }

        private void Persist()
        {
            _dbContext.Products.RemoveRange(_dbContext.Products);
            _dbContext.Products.AddRange(Products);
            _dbContext.SaveChanges();
        }

    }
}