USE [Lintas]
GO
/****** Object:  Table [dbo].[FileUploads]    Script Date: 28-05-2019 2:57:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[FileUploads](
	[Id] [uniqueidentifier] NOT NULL,
	[Ref_Id] [uniqueidentifier] NOT NULL,
	[OriginalFilename] [varchar](max) NOT NULL,
	[Description] [varchar](max) NULL,
	[Notes] [varchar](max) NULL,
 CONSTRAINT [PK_FileUploads] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
