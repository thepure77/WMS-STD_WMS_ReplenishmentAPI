﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ReplenishmentBusiness.ModelConfig
{
    public class DocumentViewModel
    {
        [Key]
        public Guid? documentItem_Index { get; set; }

        public Guid? document_Index { get; set; }

        public int? document_Status { get; set; }

        public Guid? documentItemLocation_Index { get; set; }

        public string document_No { get; set; }



        public List<DocumentViewModel> listDocumentViewModel { get; set; }
    }
}
