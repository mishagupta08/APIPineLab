using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PineAppAPI.Models
{
    public class ProductDetailContainer
    {
        public WohooProductList ProductDetail { get; set; }

        public Images ProductImages { get; set; }

        public List<ProductCategoryRelationTbl> ProductCategoryRelation { get; set; }

        public List<ProductContentTbl> ProductContentList { get; set; }

        public List<ProductDenomination> ProductDenominationList { get; set; }

        public List<ProductDiscount> ProductDiscountList { get; set; }

        public List<ProductThemeTbl> ProductThemeTblList { get; set; }

        public List<ProductList> RelatedProductList { get; set; }
    }
}